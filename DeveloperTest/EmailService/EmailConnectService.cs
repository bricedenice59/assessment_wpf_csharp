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
        private readonly IEmailConnectionPoolUtils _connectionPoolUtils;
        private readonly IEmailConnectionDescriptorInstance _sharedConnectionDescriptor;

        public IEmailConnectionPoolUtils ConnectionPoolUtils => _connectionPoolUtils;

        public EmailConnectService()
        {
            _connectionPoolUtils = ServiceLocator.Current.GetInstance<IEmailConnectionPoolUtils>();
            _sharedConnectionDescriptor = ServiceLocator.Current.GetInstance<IEmailConnectionDescriptorInstance>();
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

        /// <summary>
        /// Create multiple connections, with a  number of connections that depends on email protocol selected
        /// </summary>
        public void CreatePoolConnections()
        {
            var cd = _sharedConnectionDescriptor.GetConnectionData();
            if (cd == null)
                throw new ArgumentException("ConnectionDescriptor is null!");

            //create multiple connections, with nbConnections depending on mail protocol selected
            _connectionPoolUtils.Init(cd, cd.GetMaxConnectionsForProtocol());
        }

        /// <summary>
        /// Try to connect to host for all available connection
        /// </summary>
        /// <returns></returns>
        public async Task ConnectPooledConnectionsToHost()
        {
            foreach (var acnx in _connectionPoolUtils.GetAll())
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
        /// Try to authenticate to host for all available connection
        /// </summary>
        /// <returns></returns>
        public async Task DoAuthenticatePooledConnections()
        {
            foreach (var acnx in _connectionPoolUtils.GetAll())
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
        /// Close connection for all available connection slots
        /// </summary>
        public async Task DisconnectPooledConnectionsAsync()
        {
            foreach (var acnx in _connectionPoolUtils.GetAll())
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