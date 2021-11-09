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
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public class EmailDownloadService
    {
        private readonly ILogger _logger;
        private readonly IEmailServiceSharedContext _sharedContext;

        private int _nbProcessedHeaders;
        private int _NbProcessedBodies;

        //max number of headers we can download per connection
        private const int NumberOfDownloadHeadersInChunk = 30;

        public EmailDownloadService()
        {
            _sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();

            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

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
            foreach (var abstractConnection in _sharedContext.GetAllConnections())
            {
                if (abstractConnection is ImapConnection cnxImap)
                {
                    var imapConnectionObj = cnxImap.ImapConnectionObj;
                    _logger.Info($"Select Inbox for Connection id {abstractConnection.ConnectionId}...");
                    await imapConnectionObj.SelectInboxAsync();
                }
                else if (abstractConnection is Pop3Connection cnxPop3)
                {

                }
                else
                {
                    throw new NotImplementedException("Cannot download headers for this type of connection!");
                }
            }

            _logger.Info($"Get emails uids");
            List<long> uids = new List<long>();
            //I assume at that stage that all connections are available as this is the first action that comes straight after connecting the mail server
            if (connections[0] is ImapConnection connectionImap)
            {
                uids = await connectionImap.ImapConnectionObj.SearchAsync(Flag.All);
            }
            else if (connections[0] is Pop3Connection)
            {

            }
            else
            {
                throw new NotImplementedException("Cannot download headers for this type of connection!");
            }

            _logger.Info($"Found {uids.Count} emails to download");
            await ProcessDownloadHeadersAndBodies(uids);

            _logger.Info($"{_nbProcessedHeaders} email headers have been successfully downloaded");
            _logger.Info($"{_NbProcessedBodies} email bodies have been successfully downloaded");
        }

        private async Task ProcessDownloadHeadersAndBodies(List<long> uids)
        {
            var inputQueueHeaderDownload = new BlockingCollection<long>();
            var inputQueueBodyDownload = new BlockingCollection<long>();

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
                    maxDegreeOfParallelism: 3);
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
                        Interlocked.Increment(ref _NbProcessedBodies);
                    },
                    maxDegreeOfParallelism: 5);
            });

            //DO NOT WAIT FOR THIS TASK TO COMPLETE AS THIS WILL BLOCK BODIES TO BE DOWNLOADED CONCURRENTLY
            Task.WhenAll(t1);

            await Task.WhenAll(t2);
        }

        private async Task DownloadHeader(long emailId, AbstractConnection connection)
        {
            if (connection is ImapConnection cnx)
            {
                try
                {
                    var emailHeaderInfo = await cnx.ImapConnectionObj.GetMessageInfoByUIDAsync(emailId);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
            }
        }

        private async Task DownloadBody(long emailId, AbstractConnection connection)
        {
            if (connection is ImapConnection cnx)
            {
                var imapObj = cnx.ImapConnectionObj;
                try
                {
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