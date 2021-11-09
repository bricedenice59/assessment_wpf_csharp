using System;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ConnectionService
{
    public abstract class AbstractConnection
    {
        private long _isBusyValue = 0;

        protected ILogger Logger { get; set; }
        public int ConnectionId { get; set; }
        public ConnectionDescriptor ConnectionDescriptor { get; set; }
        public bool IsAlive { get; set; }

        public bool IsBusy
        {
            get => Interlocked.Read(ref _isBusyValue) == 1;
            set => Interlocked.Exchange(ref _isBusyValue, Convert.ToInt64(value));
        }

        public AbstractConnection(int connectionId, ConnectionDescriptor connectionDescriptor)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            Logger = loggerFactory.GetCurrentClassLogger();

            ConnectionId = connectionId;
            ConnectionDescriptor = connectionDescriptor;
        }

        public abstract Task ConnectAsync();

        public abstract Task AuthenticateAsync();

        public abstract Task DisconnectAsync();
    }
}
