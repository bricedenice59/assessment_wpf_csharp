using System;
using CommonServiceLocator;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ConnectionUtils
{
    public abstract class AbstractConnection : AbstractTimedOutConnection
    {
        private bool _connected;
        private const int ConnectTimeout = 60; //60s
        protected ILogger Logger { get; set; }
        public int ConnectionId { get; set; }
        public ConnectionDescriptor ConnectionDescriptor { get; set; }
        public bool IsAvailable { get; set; }

        public AbstractConnection(int connectionId, ConnectionDescriptor connectionDescriptor)
        {
            Logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.SetTimeout(TimeSpan.FromSeconds(ConnectTimeout));
            ConnectionId = connectionId;
            ConnectionDescriptor = connectionDescriptor;
        }

        public virtual bool IsConnected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public override void Dispose()
        {
            IsConnected = false;
            Disconnect();
        }

    }
}
