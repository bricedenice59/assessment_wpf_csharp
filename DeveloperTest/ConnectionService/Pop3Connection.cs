using System.Threading.Tasks;
using Limilabs.Client.POP3;

namespace DeveloperTest.ConnectionService
{
    public class Pop3Connection : AbstractConnection
    {
        public Pop3 Pop3ConnectionObj => _pop3ConnectionObj;

        private Pop3 _pop3ConnectionObj;

        public Pop3Connection(int connectionId, ConnectionDescriptor connectionDescriptor) :
            base(connectionId, connectionDescriptor)
        {
            IsBusy = false;
        }

        public override async Task ConnectAsync()
        {
            _pop3ConnectionObj = new Pop3();
            Logger.Info($"Connection #{ConnectionId} Try connecting to Pop3 mail server {ConnectionDescriptor.Server}:{ConnectionDescriptor.Port} :");
            switch (ConnectionDescriptor.EncryptionType)
            {
                case EncryptionTypes.SSLTLS:
                    await _pop3ConnectionObj.ConnectSSLAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    break;
                case EncryptionTypes.STARTTLS:
                    await _pop3ConnectionObj.ConnectAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    if(_pop3ConnectionObj.SupportedExtensions().Contains(Pop3Extension.STLS))
                        await _pop3ConnectionObj.StartTLSAsync();
                    break;
                case EncryptionTypes.Unencrypted:
                    await _pop3ConnectionObj.ConnectAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    break;
            }

            if (_pop3ConnectionObj.Connected)
                Logger.Info(_pop3ConnectionObj.ServerGreeting.Message);

            IsAlive = _pop3ConnectionObj.Connected;
        }

        public override async Task AuthenticateAsync()
        {
            if (!IsAlive)
            {
                Logger.Info("Cannot authenticate as connection with Pop3; connection to server is down");
                return;
            }
            Logger.Info($"Connection #{ConnectionId} Try authenticating with username and password for Pop3 mail server");

            await _pop3ConnectionObj.LoginAsync( ConnectionDescriptor.Username, ConnectionDescriptor.Password);
            Logger.Info($"Connection #{ConnectionId} Authentication OK");
        }

        public override async Task DisconnectAsync()
        {
            Logger.Info($"Connection #{ConnectionId} Try disconnecting from Pop3 mail server");
            await _pop3ConnectionObj.CloseAsync(false);
            _pop3ConnectionObj?.Dispose();
            IsAlive = false;
        }
    }
}