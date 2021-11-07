using System.Threading.Tasks;

namespace DeveloperTest.ConnectionService
{
    public class Pop3Connection : AbstractConnection
    {
        public Pop3Connection(int connectionId, ConnectionDescriptor connectionDescriptor) :
            base(connectionId, connectionDescriptor)
        {

        }

        public override async Task ConnectAsync()
        {
            Logger.Info($"Connection #{ConnectionId} Try connecting to Pop3 mail server {ConnectionDescriptor.Server}:{ConnectionDescriptor.Port} :");
            IsAlive = true;
        }

        public override Task AuthenticateAsync()
        {
            return Task.FromResult(0);
        }

        public override void Disconnect()
        {
            Logger.Info($"Connection #{ConnectionId} Try disconnecting from Pop3 mail server");
            IsAlive = false;
        }
    }
}