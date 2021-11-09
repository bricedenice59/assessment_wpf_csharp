using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using DeveloperTest.ConnectionService;
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public class EmailConnectService
    {
        private readonly ILogger _logger;

        private const int MaxActiveConnectionsImap = 5;
        private const int MaxActiveConnectionsPop3 = 3;

        public EmailConnectService(ConnectionDescriptor cd)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();

            if (cd == null)
                throw new ArgumentException("ConnectionDescriptor is null!");

            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            //create the x connections
            sharedContext.Init(cd, cd.MailProtocol == Protocols.IMAP ? MaxActiveConnectionsImap : MaxActiveConnectionsPop3);
        }

        /// <summary>
        /// Try to connect to host for all available connection slots (e.g. 5 for imap)
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToHost()
        {
            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            foreach (var acnx in sharedContext.GetAllConnections())
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
            foreach (var acnx in sharedContext.GetAllConnections())
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
        public async Task DisconnectAllOpenedConnectionAsync()
        {
            var sharedContext = ServiceLocator.Current.GetInstance<IEmailServiceSharedContext>();
            foreach (var acnx in sharedContext.GetAllConnections())
            {
                try
                {
                    await acnx.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Could not disconnect from email server!", ex);
                }
            }
        }
    }
}