using System.Runtime.InteropServices.WindowsRuntime;

namespace DeveloperTest.ConnectionService
{
    public class ConnectionDescriptor
    {
        #region Fields

        private const int MaxActiveConnectionsImap = 5;
        private const int MaxActiveConnectionsPop3 = 3;

        #endregion

        #region Properties

        public string Server { get; set; }
        public int Port { get; set; }
        public EncryptionTypes EncryptionType { get; set; }
        public Protocols MailProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        #endregion

        public int GetMaxConnectionsForProtocol()
        {
            switch (MailProtocol)
            {
                case Protocols.IMAP:
                    return MaxActiveConnectionsImap;
                case Protocols.POP3:
                    return MaxActiveConnectionsPop3;
            }

            return 1;
        }
    }
}