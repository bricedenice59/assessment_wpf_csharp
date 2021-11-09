using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using Dasync.Collections;
using DeveloperTest.ConnectionService;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public class EmailDownloadService
    {
        private readonly ILogger _logger;
        private readonly IEmailServiceSharedContext _sharedContext;

        private int _nbProcessedHeaders;
        private int _nbProcessedBodies;

        public EmailDownloadService()
        {
            _sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();

            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

        /// <summary>
        /// Download emails 
        /// </summary>
        /// <returns></returns>
        public async Task DownloadEmails()
        {
            _logger.Info("Start Download headers...");
            var connections = _sharedContext.GetAllConnections();
            if (connections == null || connections.Count == 0)
            {
                _logger.Error("No connection exist");
                throw new Exception("Cannot start download headers as no connection exist!");
            }

            //Select Inbox for all existing connections
            foreach (var abstractConnection in connections)
            {
                if (abstractConnection is ImapConnection cnxImap)
                {
                    _logger.Info($"Select Inbox for Connection id {abstractConnection.ConnectionId}...");
                    await cnxImap.ImapConnectionObj.SelectInboxAsync();
                }
                else if (abstractConnection is Pop3Connection)
                {
                    _logger.Info($"No need to select inbox for pop3 connection, go to next step...");
                }
                else
                {
                    throw new NotImplementedException("Cannot download emails for this type of connection!");
                }
            }

            _logger.Info($"Get emails uids");
            List<object> uids = new List<object>();
            //I assume at that stage that all connections are available as this is the first action that comes straight after connecting the mail server
            if (connections[0] is ImapConnection connectionImap)
            {
                var lstUidsLong = await connectionImap.ImapConnectionObj.SearchAsync(Flag.All);

                //really don't like this but requesting for emails ids for different protocols returns different object types
                //1.imap returns me a List<long>
                //2.pop3 returns me a List<string>
                uids = lstUidsLong.Cast<object>().ToList(); 
            }
            else if (connections[0] is Pop3Connection connectionPop3)
            {
                var lstUidsString = await connectionPop3.Pop3ConnectionObj.GetAllAsync();
                uids = lstUidsString.Cast<object>().ToList();
            }
            else
            {
                throw new NotImplementedException("Cannot download headers for this type of connection!");
            }

            _logger.Info($"Found {uids.Count} emails to download");
            await ProcessDownloadHeadersAndBodies(uids, connections.Count);

            _logger.Info($"{_nbProcessedHeaders} email headers have been successfully downloaded");
            _logger.Info($"{_nbProcessedBodies} email bodies have been successfully downloaded");
        }

        /// <summary>
        /// This is a method that downloads emails headers and bodies concurrently and by using the maximum connections available
        /// </summary>
        /// <param name="uids">list of emails ids to proceed with</param>
        /// <param name="maxParallelConnections">maximum number of connections to use for the download process</param>
        /// <returns></returns>
        private async Task ProcessDownloadHeadersAndBodies(List<object> uids, int maxParallelConnections)
        {
            //From testing experience, downloading email headers is always faster than downloading their bodies, hence the volume of data...
            //so I'll split concurrent connections by using 30% available connections for headers and 70% for bodies (that is true at the beginning of the process)
            //Once the headers are downloaded, all connections (100%) will be used for the unfinished task (bodies download)

            //the trick is to use the maxDegreeOfParallelism of the foreach parallel object, like for CPUs, it will create a number of parallel tasks to be run in parallel
            //At the beginning of the process, for imap connections, maximum 2 parallel tasks will process the download of headers and 3 for the bodies
            //Once headers are downloaded, then the second method that download bodies can use the maximum 5 connections for faster download
            //For every task, one available connection "not busy yet" will be assigned to it, once task has completed, the "busy"connection is released and can be reused for the next task

            int maxParallelConnectionsForHeaders = 1;
            //case POP3
            if (maxParallelConnections == 3)
                maxParallelConnectionsForHeaders = 1;

            //case IMAP
            else if (maxParallelConnections == 5)
                maxParallelConnectionsForHeaders = 2;

            var inputQueueHeaderDownload = new BlockingCollection<object>();
            var inputQueueBodyDownload = new BlockingCollection<object>();

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
                            availableCnx = _sharedContext.GetOneAvailableConnection();
                            if (availableCnx == null)
                            {
#if DEBUG
                                _logger.Info("No connection for downloading header"+ uid);
                                await Task.Delay(20);
#endif
                                continue;
                            }

#if DEBUG
                            _logger.Info("Connection available for downloading header"+ uid);
#endif
                            break;
                        }
                        await DownloadHeader(uid, availableCnx);
                        _sharedContext.FreeBusyConnection(availableCnx);
                        Interlocked.Increment(ref _nbProcessedHeaders);
                        inputQueueBodyDownload.Add(uid);
                    },
                    maxDegreeOfParallelism: maxParallelConnectionsForHeaders);
                inputQueueBodyDownload.CompleteAdding();
            });

            Task t2 = Task.Run(async () =>
            {
                await inputQueueBodyDownload.GetConsumingEnumerable().ParallelForEachAsync(
                    async uid =>
                    {
                        AbstractConnection availableCnx;
                        while (true)
                        {
                            availableCnx = _sharedContext.GetOneAvailableConnection();
                            if (availableCnx == null)
                            {
#if DEBUG
                                _logger.Info("No connection for downloading body" + uid);
                                await Task.Delay(20);
#endif
                                continue;
                            }

#if DEBUG
                            _logger.Info("Connection available for downloading body" + uid);
#endif
                            break;
                        }

                        await DownloadBody(uid, availableCnx);
                        _sharedContext.FreeBusyConnection(availableCnx);
                        Interlocked.Increment(ref _nbProcessedBodies);
                    },
                    maxDegreeOfParallelism: maxParallelConnections);
            });

            //DO NOT WAIT FOR THIS TASK TO COMPLETE AS THIS WILL BLOCK BODIES TO BE DOWNLOADED CONCURRENTLY
            Task.WhenAll(t1);

            await Task.WhenAll(t2);
        }

        private async Task DownloadHeader(object emailIdObj, AbstractConnection connection)
        {
            if (connection is ImapConnection imapCnx)
            {
                try
                {
                    var emailId = (long)emailIdObj;
                    var emailHeaderInfo = await imapCnx.ImapConnectionObj.GetMessageInfoByUIDAsync(emailId);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }
            else if (connection is Pop3Connection pop3Cnx)
            { //Pop3 connection
                try
                {
                    MailBuilder builder = new MailBuilder();
                    var emailId = emailIdObj.ToString();
                    var emailHeaderInfo = await pop3Cnx.Pop3ConnectionObj.GetHeadersByUIDAsync(emailId);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }
        }

        private async Task DownloadBody(object emailIdObj, AbstractConnection connection)
        {
            if (connection is ImapConnection cnx)
            {
                var imapObj = cnx.ImapConnectionObj;
                try
                {
                    var emailId = (long)emailIdObj;
                    var emailBodyStruct = await imapObj.GetBodyStructureByUIDAsync(emailId);
                    // Download only text and html parts
                    string text = null, html = null;

                    if (emailBodyStruct.Text != null)
                        text = await imapObj.GetTextByUIDAsync(emailBodyStruct.Text);
                    if (emailBodyStruct.Html != null)
                        html = await imapObj.GetTextByUIDAsync(emailBodyStruct.Html);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email header", e);
                }
            }
        }
    }
}