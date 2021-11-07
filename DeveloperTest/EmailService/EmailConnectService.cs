using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonServiceLocator;
using DeveloperTest.ConnectionService;
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public class EmailConnectService
    {
        private readonly ILogger _logger;

        public EmailConnectService(int maxActiveConnections, ConnectionDescriptor cd)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();

            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            sharedContext.Init(maxActiveConnections);
            var connections = sharedContext.GetAvailableConnections();

            for (int connectionNb = 0; connectionNb < maxActiveConnections; connectionNb++)
            {
                AbstractConnection cxn = cd.MailProtocol == Protocols.IMAP
                    ? new ImapConnection(connectionNb, cd)
                    : (AbstractConnection)new Pop3Connection(connectionNb, cd);
                connections.Add(cxn);
            }
        }

        /// <summary>
        /// Try to connect to host for all available connection slots (e.g. 5 for imap)
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToHost()
        {
            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            foreach (var acnx in sharedContext.GetAvailableConnections())
            {
                try
                {
                    await acnx.ConnectAsync();
                }
                catch (Limilabs.Client.ServerException serverException)
                {
                    _logger.ErrorException("Could not connect to email server!", serverException);

                    //There is no reason to continue trying opening other connections as they will all end in failure
                    break;
                }
            }
        }

        /// <summary>
        /// Try to authenticate to host for all available connection slots (e.g. 5 for imap)
        /// </summary>
        /// <returns></returns>
        public async Task DoAuthenticate()
        {
            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            foreach (var acnx in sharedContext.GetAvailableConnections())
            {
                try
                {
                    await acnx.AuthenticateAsync();
                }
                catch (Limilabs.Client.ServerException serverException)
                {
                    _logger.ErrorException("Authentication failed!", serverException);

                    //There is no reason to continue trying authenticate for other connections as they will all end in failure
                    throw;
                }
            }
        }

        /// <summary>
        /// Close connection for all available connection slots (e.g. 5 for imap)
        /// </summary>
        public void Stop()
        {
            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            foreach (var acnx in sharedContext.GetAvailableConnections())
            {
                try
                {
                    acnx.Disconnect();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Could not disconnect from email server!", ex);
                }
            }
        }
    }
}