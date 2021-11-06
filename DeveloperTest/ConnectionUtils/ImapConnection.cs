namespace DeveloperTest.ConnectionUtils
{
    public class ImapConnection : AbstractConnection
    {
        public ImapConnection(int connectionId, ConnectionDescriptor connectionDescriptor) : 
            base(connectionId, connectionDescriptor)
        {

        }

        public override void Connect()
        {
            this.Logger.Info($"Connection #{ConnectionId} Try connecting to Imap mail server {ConnectionDescriptor.Server}:{ConnectionDescriptor.Port} :");
            IsConnected = true;
            IsAvailable = true;
        }

        public override void Disconnect()
        {
            this.Logger.Info($"Connection #{ConnectionId} Try disconnecting Imap from mail server");
            IsConnected = false;
            IsAvailable = false;
        }
    }
}