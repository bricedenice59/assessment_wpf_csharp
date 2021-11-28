using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp.DevTools.Runtime;
using CommonServiceLocator;
using Dasync.Collections;
using DeveloperTest.ConnectionService;
using DeveloperTest.Utils;
using DeveloperTest.Utils.Events;
using DeveloperTest.ValueObjects;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public enum ScanProgress
    {
        InProgress,
        Completed
    }

    public class EmailDownloadService : IEmailDownloadService
    {
        public event EventHandler<ScanEmailsStatusChangedEventArgs> ScanEmailsStatusChanged;
        public event EventHandler<NewEmailDiscoveredEventArgs> NewEmailDiscovered;
        private readonly ILogger _logger;
        private readonly IEmailConnectionUtils _connectionUtils;

        private static object _lockEmailBodyProcess = new object();
        private int _nbProcessedHeaders;
        private int _nbProcessedBodies;
        private SemaphoreSlim _semaphoreSlim;

        //contains the list of body uids that have been already downloaded, this property is used to avoid processing body download more than once
        public ConcurrentBag<string> ProcessedBodies { get; set; }

        public EmailDownloadService(ILogger logger, IEmailConnectionUtils connectionUtils)
        {
            _logger = logger;
            _connectionUtils = connectionUtils;
            ProcessedBodies = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Download emails 
        /// </summary>
        /// <returns></returns>
        public async Task DownloadEmails()
        {
            _logger.Info("Start Download headers...");
            var connections = _connectionUtils.GetAll();
            if (connections == null || connections.Count == 0)
            {
                _logger.Error("No connection exist");
                throw new Exception("Cannot start download headers as no connection exist!");
            }

            //Select Inbox for all existing connections
            foreach (var connection in connections)
            {
                _logger.Info($"Select Inbox for Connection id {connection.ConnectionId}...");
                if(await _connectionUtils.SelectInboxAsync(connection))
                {
                    // at that stage we have a valid connection ready for downloading emails
                    //add it in the connection pool
                    _connectionUtils.Enqueue(connection);
                }
            }

            _logger.Info($"Get emails uids");
            List<string> uids = new List<string>();
            //I assume at that stage that all connections are available as this is the first action that comes straight after connecting the mail server
            if (connections[0] is ImapConnection connectionImap)
            {
                var lstUidsLong = await connectionImap.ImapConnectionObj.SearchAsync(Flag.All);

                //really don't like this but requesting for emails ids for different protocols returns different object types
                //1.imap returns me a List<long>
                //2.pop3 returns me a List<string>
                lstUidsLong.ForEach(x=> uids.Add(x.ToString()));

                _logger.Info($"API returns emails sorted from oldest to newest");
                _logger.Info($"Let's sort them in the opposite way(from newest to oldest) right now, so we don't need to deal with it later in UI");

                //for my testings on
                //1.retrieving gmail emails with imap protocol returns me emails sorted from newest to oldest
                //2.retrieving hotmail emails with imap protocol returns me emails sorted from oldest to newest
                //what is going with this API, that's a bug or the API documentation is wrong

                //reverse randomly ? hihi :)
                uids.Reverse();
            }
            else if (connections[0] is Pop3Connection connectionPop3)
            {
                uids = await connectionPop3.Pop3ConnectionObj.GetAllAsync();
            }
            else
            {
                throw new NotImplementedException("Cannot download headers for this type of connection!");
            }

            _logger.Info($"Found {uids.Count} emails to download");

            ScanEmailsStatusChanged?.Invoke(this, new ScanEmailsStatusChangedEventArgs(ScanProgress.InProgress));
            _semaphoreSlim = new SemaphoreSlim(connections.Count);
            await ProcessDownloadHeadersAndBodies1(uids);
            ScanEmailsStatusChanged?.Invoke(this, new ScanEmailsStatusChangedEventArgs(ScanProgress.Completed));

            _logger.Info($"{_nbProcessedHeaders} email headers have been successfully downloaded");
            _logger.Info($"{_nbProcessedBodies} email bodies have been successfully downloaded");
        }

        /// <summary>
        /// This is a method that downloads emails headers and bodies concurrently and by using the maximum connections available
        /// </summary>
        /// <param name="uids">list of emails ids to proceed with</param>
        /// <returns></returns>
        private async Task ProcessDownloadHeadersAndBodies1(List<string> uids)
        {
            var tasksDownload = uids
                .Select(x => Task.Run(async () =>
                {
                    await _semaphoreSlim.WaitAsync();

                    var availableCnx = _connectionUtils.GetOneAvailable();
                    if (availableCnx == null)
                    {
                        //that should never happen since the semaphore initial count = number of connections in pool
                        _logger.Info("No connection available");
                        return;
                    }

                    var email = await DownloadHeader(x, availableCnx);
                    Interlocked.Increment(ref _nbProcessedHeaders);

                    //send downloaded header to UI
                    if (email != null)
                        NewEmailDiscovered?.Invoke(this, new NewEmailDiscoveredEventArgs(email));

                    await DownloadBody(email, availableCnx);
                    Interlocked.Increment(ref _nbProcessedBodies);

                    //free connection and re-enqueue it
                    _connectionUtils.Enqueue(availableCnx);

                    _semaphoreSlim.Release();
                }))
                .ToArray();

            await Task.WhenAll(tasksDownload);
        }

        private async Task<EmailObject> DownloadHeader(string emailIdObj, AbstractConnection connection)
        {
            if (connection is ImapConnection imapCnx)
            {
                try
                {
                    var emailId = long.Parse(emailIdObj);
                    var emailHeaderInfo = await imapCnx.ImapConnectionObj.GetMessageInfoByUIDAsync(emailId);

                   return new EmailObject()
                    {
                        Uid = emailIdObj,
                        From = emailHeaderInfo.Envelope.From.FirstOrDefault()?.Address,
                        Date = emailHeaderInfo.Envelope.Date?.ToString(),
                        Subject = emailHeaderInfo.Envelope.Subject
                    };
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }
            else if (connection is Pop3Connection pop3Cnx)
            {
                //Pop3 connection
                try
                {
                    MailBuilder builder = new MailBuilder();
                    var emailHeaderInfo = await pop3Cnx.Pop3ConnectionObj.GetHeadersByUIDAsync(emailIdObj);
                    IMail email = builder.CreateFromEml(emailHeaderInfo);
                    return new EmailObject
                    {
                        Uid = emailIdObj,
                        From = email.From.FirstOrDefault()?.Name,
                        Date = email.Date?.ToString(),
                        Subject = email.Subject
                    };
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }
            else throw new NotImplementedException("Protocol not supported!");

            return null;
        }

        public async Task DownloadBody(EmailObject emailObj, AbstractConnection connection)
        {
            if (connection == null || 
                (connection.GetType() != typeof(ImapConnection) && connection.GetType() != typeof(Pop3Connection)))
                return;

            //don't download email body if there is already a task that does the job
            //this happens if a request on downloading on demand has been raised and at the same time the automatic download is still running
            if (HasEmailBodyBeenProcessed(emailObj))
            {
                emailObj.SetBodyIsNowDownloaded();
                return;
            }

            AddProcessedBodyToBag(emailObj);

            if (connection is ImapConnection cnx)
            {
                var imapObj = cnx.ImapConnectionObj;
                try
                {
                    var emailId = long.Parse(emailObj.Uid);
                    var emailBodyStruct = await imapObj.GetBodyStructureByUIDAsync(emailId);
                    // Download only text and html parts
                    string text = null, html = null;

                    if (emailBodyStruct.Text != null)
                        text = await imapObj.GetTextByUIDAsync(emailBodyStruct.Text);
                    if (emailBodyStruct.Html != null)
                        html = await imapObj.GetTextByUIDAsync(emailBodyStruct.Html);

                    //some conversion work for later displaying html instead of text
                    emailObj.Body = html ?? HtmlUtils.GetHtmlFromText(text);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email header", e);
                }
            }
            else if (connection is Pop3Connection pop3Cnx)
            { //Pop3 connection
                var pop3Obj = pop3Cnx.Pop3ConnectionObj;
                try
                {
                    MailBuilder builder = new MailBuilder();
                    var resMessage = await pop3Obj.GetMessageByUIDAsync(emailObj.Uid);
                    IMail email = builder.CreateFromEml(resMessage);

                    // Download only text and html parts
                    string text = null, html = null;

                    if (email.Text != null)
                        text = email.Text;
                    if (email.Html != null)
                        html = email.Html;

                    emailObj.Body = html ?? HtmlUtils.GetHtmlFromText(text);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }

            emailObj.SetBodyIsNowDownloaded();
        }

        private bool HasEmailBodyBeenProcessed(EmailObject emailObj)
        {
            lock(_lockEmailBodyProcess)
            {
                return ProcessedBodies.Any(x => x == emailObj.Uid);
            }
        }

        private void AddProcessedBodyToBag(EmailObject emailObj)
        {
            lock (_lockEmailBodyProcess)
            {
                ProcessedBodies.Add(emailObj.Uid);
            }
        }
    }
}