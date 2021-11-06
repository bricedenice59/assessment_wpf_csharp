using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ConnectionUtils
{
    public abstract class AbstractConnection : IDisposable
    {
        protected ILogger Logger { get; set; }
        public int ConnectionId { get; set; }
        public ConnectionDescriptor ConnectionDescriptor { get; set; }
        public bool IsAlive { get; set; }

        public AbstractConnection(int connectionId, ConnectionDescriptor connectionDescriptor)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            Logger = loggerFactory.GetCurrentClassLogger();

            ConnectionId = connectionId;
            ConnectionDescriptor = connectionDescriptor;
        }

        public abstract Task<bool> ConnectAsync();

        public abstract Task AuthentificateAsync();

        public abstract void Disconnect();

        public void Dispose()
        {
            Disconnect();
        }
    }
}
