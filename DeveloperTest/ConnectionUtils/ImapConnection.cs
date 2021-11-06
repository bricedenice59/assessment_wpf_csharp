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

            return IsAlive = _imapConnectionObj.Connected;
        }

        public override void Disconnect()
        {
            Logger.Info($"Connection #{ConnectionId} Try disconnecting from Imap mail server");
            _imapConnectionObj?.Dispose();
            IsAlive = false;
        }
    }
}