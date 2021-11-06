using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Limilabs.Client.IMAP;
using Limilabs.Mail;

namespace DeveloperTest.ConnectionUtils
{
    public class ImapConnection : AbstractConnection
    {
        private Imap _imapConnectionObj;

        public ImapConnection(int connectionId, ConnectionDescriptor connectionDescriptor) : 
            base(connectionId, connectionDescriptor)
        {

        }

        public override async Task<bool> ConnectAsync()
        {
            Logger.Info($"Connection #{ConnectionId} Try connecting to Imap mail server {ConnectionDescriptor.Server}:{ConnectionDescriptor.Port} :");

            _imapConnectionObj = new Imap();
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

            return IsAlive = _imapConnectionObj.Connected;
        }

        public override async Task AuthentificateAsync()
        {
            if (!IsAlive)
            {
                Logger.Info("Cannot authenticate as connection to Imap mail server is down");
                return;
            }
            Logger.Info($"Connection #{ConnectionId} Try authenticating with username and password for Imap mail server");

            await _imapConnectionObj.UseBestLoginAsync(ConnectionDescriptor.Username, ConnectionDescriptor.Password);
            Logger.Info($"Connection #{ConnectionId} Authentication OK");
        }

        public override void Disconnect()
        {
            Logger.Info($"Connection #{ConnectionId} Try disconnecting from Imap mail server");
            _imapConnectionObj?.Dispose();
            IsAlive = false;
        }
    }
}