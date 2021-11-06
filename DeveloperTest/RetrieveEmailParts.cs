using System;
using System.Collections.Generic;
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
        private ILogger Logger { get; set; }

        public RetrieveEmailPartsProcess(int maxActiveConnections, ConnectionDescriptor cd)
        {
            Logger = ServiceLocator.Current.GetInstance<ILogger>();
            _maxActiveConnections = maxActiveConnections;
            _connectionDescriptor = cd;
            _connections = new List<AbstractConnection>(_maxActiveConnections);
        }

        public void Start()
        {
            for (int connectionNb = 0; connectionNb < _maxActiveConnections; connectionNb++)
            {
                AbstractConnection cxn = _connectionDescriptor.EncryptionProtocol == Protocols.IMAP
                    ? new ImapConnection(connectionNb, _connectionDescriptor)
                    : (AbstractConnection)new Pop3Connection(connectionNb, _connectionDescriptor);
                _connections.Add(cxn);
            }

            foreach (AbstractConnection acnx in _connections)
            {
                try
                {
                    acnx.Connect();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Could not connect to email server!", ex);
                }
            }
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
                    Logger.ErrorException("Could not disconnect from email server!", ex);
                }
            }
        }
    }
}