using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using DeveloperTest.ConnectionUtils;
using Ninject.Extensions.Logging;

namespace DeveloperTest
{
    public class RetrieveEmailPartsProcess
    {
        private readonly int _maxActiveConnections;
        private readonly ConnectionDescriptor _connectionDescriptor;
        private List<AbstractConnection> _connections;
        private ILogger _logger;

        public RetrieveEmailPartsProcess(int maxActiveConnections, ConnectionDescriptor cd)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();

            _maxActiveConnections = maxActiveConnections;
            _connectionDescriptor = cd;
            _connections = new List<AbstractConnection>(_maxActiveConnections);

            for (int connectionNb = 0; connectionNb < _maxActiveConnections; connectionNb++)
            {
                AbstractConnection cxn = _connectionDescriptor.MailProtocol == Protocols.IMAP
                    ? new ImapConnection(connectionNb, _connectionDescriptor)
                    : (AbstractConnection)new Pop3Connection(connectionNb, _connectionDescriptor);
                _connections.Add(cxn);
            }
        }

        public async Task<bool> ConnectToHost()
        {
            bool success = true;
            foreach (var acnx in _connections)
            {
                try
                {
                    success = await acnx.ConnectAsync();
                }
                catch (Limilabs.Client.ServerException serverException)
                {
                    success = false;
                    _logger.ErrorException("Could not connect to email server!", serverException);

                    //There is no reason to continue trying opening other connections as they will all end in failure
                    break;
                }
            }

            return success;
        }

        public async Task<bool> DoAuthenticate()
        {
            return true;
        }

        public void Stop()
        {
            foreach (AbstractConnection acnx in _connections)
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