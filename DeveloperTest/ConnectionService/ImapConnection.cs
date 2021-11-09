using System.Threading.Tasks;
using Limilabs.Client.IMAP;

namespace DeveloperTest.ConnectionService
{
    public class ImapConnection : AbstractConnection
    {
        public Imap ImapConnectionObj => _imapConnectionObj;

        private Imap _imapConnectionObj;


        public ImapConnection(int connectionId, ConnectionDescriptor connectionDescriptor) : 
            base(connectionId, connectionDescriptor)
        {
            IsBusy = false;
        }

        /// <summary>
        /// Initiate connection to Email server 
        /// </summary>
        /// <returns></returns>
        public override async Task ConnectAsync()
        {
            _imapConnectionObj = new Imap();

            Logger.Info($"Connection #{ConnectionId} Try connecting to Imap mail server {ConnectionDescriptor.Server}:{ConnectionDescriptor.Port} :");

            switch (ConnectionDescriptor.EncryptionType)
            {
                case EncryptionTypes.SSLTLS:
                    await _imapConnectionObj.ConnectSSLAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    break;
                case EncryptionTypes.STARTTLS:
                    await _imapConnectionObj.ConnectAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    await _imapConnectionObj.StartTLSAsync();
                    break;
                case EncryptionTypes.Unencrypted:
                    await _imapConnectionObj.ConnectAsync(ConnectionDescriptor.Server, ConnectionDescriptor.Port);
                    break;
            }

            if(_imapConnectionObj.Connected)
                Logger.Info(_imapConnectionObj.ServerGreeting.Message);

            IsAlive = _imapConnectionObj.Connected;
        }

        /// <summary>
        /// Authenticate with email server with provided credentials  
        /// </summary>
        /// <returns></returns>
        public override async Task AuthenticateAsync()
        {
            if (!IsAlive)
            {
                Logger.Info("Cannot authenticate as connection with Imap; connection to server is down");
                return;
            }
            Logger.Info($"Connection #{ConnectionId} Try authenticating with username and password for Imap mail server");

            await _imapConnectionObj.UseBestLoginAsync(ConnectionDescriptor.Username, ConnectionDescriptor.Password);
            Logger.Info($"Connection #{ConnectionId} Authentication OK");
        }

        public override async Task DisconnectAsync()
        {
            Logger.Info($"Connection #{ConnectionId} Try disconnecting from Imap mail server");
            await _imapConnectionObj.CloseAsync(false);
            _imapConnectionObj?.Dispose();
            IsAlive = false;
        }
    }
}