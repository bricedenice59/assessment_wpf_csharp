using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
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

            //I assume at that stage that all connections are available as this is the first action that comes straight after connecting the mail server
            if (connections[0] is ImapConnection connectionImap)
            {
                _logger.Info($"Get emails uids");
                var uids = await connectionImap.ImapConnectionObj.SearchAsync(Flag.All);
                await ProcessDownloadHeadersAndBodies(uids, connections);

                _logger.Info($"{_nbProcessedHeaders} have been successfully downloaded");
                _logger.Info($"{_NbProcessedBodies} have been successfully downloaded");
            }
            else if (connections[0] is Pop3Connection)
            {

            }
            else
            {
                throw new NotImplementedException("Cannot download headers for this type of connection!");
            }
        }

        private async Task ProcessDownloadHeadersAndBodies(List<long> uids, List<AbstractConnection> connections)
        {
            var inputQueueHeaderDownload = new BlockingCollection<long>();
            var inputQueueBodyDownload = new BlockingCollection<long>();

            //feed the queue with all email ids we need to download headers for
            foreach (var id in uids)
                inputQueueHeaderDownload.Add(id);

            inputQueueHeaderDownload.CompleteAdding();

            var tasksListDownloadHeaders = connections.Take(2)
                .Select(x => Task.Run(async () =>
                {
                    foreach (var uid in inputQueueHeaderDownload.GetConsumingEnumerable())
                    {
                        await DownloadHeader(uid, x);

                        Interlocked.Increment(ref _nbProcessedHeaders);
                        //Console.WriteLine($"Nb Processed Header : {_nbProcessedHeaders}");
                        inputQueueBodyDownload.Add(uid);
                    }
                    inputQueueBodyDownload.CompleteAdding();
                }));

            var tasksListDownloadBodies = WaitForConnectionSlotToBeAvailable().Skip(2).Take(3)
                .Select(x => Task.Run(async () =>
                {
                    foreach (var uid in inputQueueBodyDownload.GetConsumingEnumerable())
                    {
                        await DownloadBody(uid, x);

                        Interlocked.Increment(ref _NbProcessedBodies);
                        //Console.WriteLine($"Nb Processed Body : {_NbProcessedBodies}");
                    }
                }));

            //DO NOT WAIT FOR THIS TASK TO COMPLETE AS THIS WILL BLOCK BODIES TO BE DOWNLOADED CONCURRENTLY
            Task.WhenAll(tasksListDownloadHeaders);

            await Task.WhenAll(tasksListDownloadBodies);
        }

        private async Task DownloadHeader(long emailId, AbstractConnection connection)
        {
            if (connection is ImapConnection cnx)
            {
                try
                {
                    connection.IsBusy = true;
                    var emailHeaderInfo = await cnx.ImapConnectionObj.GetMessageInfoByUIDAsync(emailId);

                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error occurred when trying to download email body", e);
                }
                finally
                {
                    connection.IsBusy = false;
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
                    connection.IsBusy = true;
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
                finally
                {
                    connection.IsBusy = false;
                }
            }
        }

        private IEnumerable<AbstractConnection> WaitForConnectionSlotToBeAvailable()
        {
            var availableCnxs = _sharedContext.GetAllAvailableConnections();
            foreach (var availableCnx in availableCnxs)
            {
                _logger.Info($"Connection Id {availableCnx.ConnectionId} is available...");
            }

            return availableCnxs;
        }
    }
}