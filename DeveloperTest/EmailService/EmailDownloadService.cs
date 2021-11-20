using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private int _nbProcessedHeaders;
        private int _nbProcessedBodies;

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
                await _connectionUtils.SelectInboxAsync(connection);
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
            await ProcessDownloadHeadersAndBodies(uids, connections.Count);
            ScanEmailsStatusChanged?.Invoke(this, new ScanEmailsStatusChangedEventArgs(ScanProgress.Completed));

            _logger.Info($"{_nbProcessedHeaders} email headers have been successfully downloaded");
            _logger.Info($"{_nbProcessedBodies} email bodies have been successfully downloaded");
        }

        /// <summary>
        /// This is a method that downloads emails headers and bodies concurrently and by using the maximum connections available
        /// </summary>
        /// <param name="uids">list of emails ids to proceed with</param>
        /// <param name="maxParallelConnections">maximum number of connections to use for the download process</param>
        /// <returns></returns>
        private async Task ProcessDownloadHeadersAndBodies(List<string> uids, int maxParallelConnections)
        {
            //From testing experience, downloading email headers is always faster than downloading their bodies, hence the volume of data...
            //so I'll split concurrent connections by using 30% available connections for headers and 70% for bodies (that is true at the beginning of the process)
            //Once the headers are downloaded, all connections (100%) will be used for the unfinished task (bodies download)

            //the trick is to use the maxDegreeOfParallelism of the foreach parallel object, like for CPUs, it will create a number of parallel tasks to be run in parallel
            //At the beginning of the process, for imap connections, maximum 4 parallel tasks will process the download of headers and 1 for the bodies
            //Once headers are downloaded, then the second method that download bodies can use the maximum 5 connections for faster download
            //For every task, one available connection "not busy yet" will be assigned to it, once task has completed, the "busy"connection is released and can be reused for the next task

            int maxParallelConnectionsForHeaders = 1;
            //case POP3
            if (maxParallelConnections == 3)
                maxParallelConnectionsForHeaders = 2;

            //case IMAP
            else if (maxParallelConnections == 5)
                maxParallelConnectionsForHeaders = 4;

            var inputQueueHeaderDownload = new BlockingCollection<string>();
            var inputQueueBodyDownload = new BlockingCollection<EmailObject>();

            //feed the queue with all email ids we need to download headers for
            foreach (var id in uids)
                inputQueueHeaderDownload.Add(id);

            inputQueueHeaderDownload.CompleteAdding();

            Task t1 = Task.Run(async() =>
            {
                await inputQueueHeaderDownload.GetConsumingEnumerable().ParallelForEachAsync(
                    async uid =>
                    {
                        AbstractConnection availableCnx;
                        while (true)
                        {
                            availableCnx = _connectionUtils.GetOneAvailable();
                            if (availableCnx == null)
                            {
#if DEBUG
                                _logger.Debug("No connection for downloading header"+ uid);
                                await Task.Delay(20);
#endif
                                continue;
                            }

#if DEBUG
                            _logger.Debug("Connection available for downloading header"+ uid);
#endif
                            break;
                        }
                        var email = await DownloadHeader(uid, availableCnx);

                        _connectionUtils.FreeBusy(availableCnx);
                        Interlocked.Increment(ref _nbProcessedHeaders);
                        //downloading headers is done
                        //add the current mail to the queue of the next processing tasks
                        inputQueueBodyDownload.Add(email);

                        //send downloaded header to UI
                        if(email != null)
                            NewEmailDiscovered?.Invoke(this, new NewEmailDiscoveredEventArgs(email));
                    },
                    maxDegreeOfParallelism: maxParallelConnectionsForHeaders);
                inputQueueBodyDownload.CompleteAdding();
            });

            Task t2 = Task.Run(async () =>
            {
                await inputQueueBodyDownload.GetConsumingEnumerable().ParallelForEachAsync(
                    async email =>
                    {
                        AbstractConnection availableCnx;
                        while (true)
                        {
                            availableCnx = _connectionUtils.GetOneAvailable();
                            if (availableCnx == null)
                            {
#if DEBUG
                                _logger.Debug("No connection for downloading body" + email.Uid);
                                await Task.Delay(20);
#endif
                                continue;
                            }
#if DEBUG
                            _logger.Debug("Connection available for downloading header" + email.Uid);
#endif

                            break;
                        }

                        await DownloadBody(email, availableCnx);

                        _connectionUtils.FreeBusy(availableCnx);
                        Interlocked.Increment(ref _nbProcessedBodies);
                    },
                    maxDegreeOfParallelism: maxParallelConnections);
            });

            //DO NOT WAIT FOR THIS TASK TO COMPLETE AS THIS WILL BLOCK BODIES TO BE DOWNLOADED CONCURRENTLY
            Task.WhenAll(t1);

            await Task.WhenAll(t2);
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
            if (ProcessedBodies.Any(x => x == emailObj.Uid))
            {
                emailObj.SetBodyIsNowDownloaded();
                return;
            }

            ProcessedBodies.Add(emailObj.Uid);

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
    }
}