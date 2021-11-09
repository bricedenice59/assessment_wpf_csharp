using System.Collections.Generic;
using System.Linq;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public class EmailServiceSharedContext : IEmailServiceSharedContext
    {
        private static object _lock = new object();
        private List<AbstractConnection> _connections;

        public EmailServiceSharedContext()
        {
            _connections = new List<AbstractConnection>();
        }

        public void Init(ConnectionDescriptor cd, int nbConnections)
        {
            for (int i = 0; i < nbConnections; i++)
            {
                if (cd.MailProtocol == Protocols.IMAP)
                    _connections.Add(new ImapConnection(i, cd));
                else if (cd.MailProtocol == Protocols.POP3)
                    _connections.Add(new Pop3Connection(i, cd));
            }
        }

        public List<AbstractConnection> GetAllConnections()
        {
            return _connections;
        }

        public AbstractConnection GetOneAvailableConnection()
        {
            lock (_lock)
            {
                var cnx= _connections.FirstOrDefault(x => !x.IsBusy);
                if(cnx != null)
                    cnx.IsBusy = true;
                return cnx;
            }
        }

        public void FreeBusyConnection(AbstractConnection ac)
        {
            lock (_lock)
            {
                ac.IsBusy = false;
            }
        }
    }
}