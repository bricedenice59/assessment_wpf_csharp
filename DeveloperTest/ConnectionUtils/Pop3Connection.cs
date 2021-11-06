namespace DeveloperTest.ConnectionUtils
{
    public class Pop3Connection : AbstractConnection
    {
        public Pop3Connection(int connectionId, ConnectionDescriptor connectionDescriptor) :
            base(connectionId, connectionDescriptor)
        {

        }

        public override void Connect()
        {
            IsConnected = true;
            IsAvailable = IsAlive();
        }

        public override void Disconnect()
        {
            IsConnected = false;
            IsAvailable = IsAlive();
        }
    }
}