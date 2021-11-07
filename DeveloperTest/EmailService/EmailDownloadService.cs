using System;
using System.Collections.Generic;
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

        public EmailDownloadService()
        {
            _sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();

            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

        public async Task DownloadHeaders()
        {
            _logger.Info("Start Download headers...");
            var connections = _sharedContext.GetAvailableConnections();
            if (connections == null || connections?.Count == 0)
            {
                _logger.Error("No connection exist");
                throw new Exception("Cannot start download headers as no connection exist!");
            }

            //I assume at that stage that all connections are available as this is the first action that comes straight after connecting the mail server
            var mailServerType = connections[0];
            if (mailServerType is ImapConnection)
            {
                var imapConnectionObj = ((ImapConnection) mailServerType).ImapConnectionObj;

                _logger.Info("Select Inbox...");
                var resSelectInbox = await imapConnectionObj.SelectInboxAsync();
                _logger.Info($"Found {resSelectInbox.MessageCount} emails");
                if (resSelectInbox.MessageCount > 0)
                {
                    _logger.Info($"Get emails uids");
                    var uids = await imapConnectionObj.SearchAsync(Flag.All);

                    List<MessageInfo> infos = await imapConnectionObj.GetMessageInfoByUIDAsync(uids);
                }

            }
            else if (mailServerType is Pop3Connection)
            {

            }
            else
            {
                throw new NotImplementedException("Cannot download headers for this type of connection!");
            }
        }

        public async Task DownloadBody(long emailId)
        {

        }
    }
}