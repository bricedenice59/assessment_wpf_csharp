using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeveloperTest.ConnectionService;
using Ninject.Extensions.Logging;

namespace DeveloperTest.EmailService
{
    public class EmailConnectionUtils : IEmailConnectionUtils
    {
        private static object _lock = new object();
        private readonly ILogger _logger;
        private readonly IEmailConnectionDescriptorInstance _sharedConnectionDescriptor;
        private ConcurrentQueue<AbstractConnection> _connectionsPool;

        public EmailConnectionUtils(IEmailConnectionDescriptorInstance sharedConnectionDescriptor,
            ILogger logger)
        {
            _sharedConnectionDescriptor = sharedConnectionDescriptor;
            _logger = logger;
            _connectionsPool = new ConcurrentQueue<AbstractConnection>();
        }

        #region Create Connections

        /// <summary>
        /// Create multiple connections, with a  number of connections that depends on email protocol selected
        /// </summary>
        public List<AbstractConnection> CreateConnections()
        {
            var cnxs = new List<AbstractConnection>();

            var cd = _sharedConnectionDescriptor.GetConnectionData();
            if (cd == null)
                throw new ArgumentException("ConnectionDescriptor is null!");

            for (int i = 1; i <= cd.GetMaxConnectionsForProtocol(); i++)
            {
                var cnx = CreateOneConnection(i);
                cnxs.Add(cnx);
            }

            return cnxs;
        }

        /// <summary>
        /// Create one connection
        /// </summary>
        /// <returns></returns>
        public AbstractConnection CreateOneConnection(int? idConnection)
        {
            var cd = _sharedConnectionDescriptor.GetConnectionData();
            if (cd == null)
                throw new ArgumentException("ConnectionDescriptor is null!");

            //create one connection
            return CreateNewConnection(cd, idConnection);
        }

        /// <summary>
        /// Create a new connection instance for given protocol (Imap or Pop3)
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        private AbstractConnection CreateNewConnection(ConnectionDescriptor cd, int? idConnection)
        {
            if (!idConnection.HasValue)
            {
                if (_sharedConnectionDescriptor.ConnectionsList?.Count > 0)
                    idConnection = _sharedConnectionDescriptor.ConnectionsList.Count + 1;
                else idConnection = -1;
            }

            if (cd.MailProtocol == Protocols.IMAP)
                return new ImapConnection(idConnection.Value, cd);
            if (cd.MailProtocol == Protocols.POP3)
                return new Pop3Connection(idConnection.Value, cd);

            return null;
        }

        #endregion

        #region Disconnect

        /// <summary>
        /// Disconnect connections
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync(List<AbstractConnection> cnxs)
        {
            foreach (var cnx in cnxs)
            {
                try
                {
                    await cnx.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"Could not disconnect connection id {cnx.ConnectionId} from email server!", ex);
                }
            }
        }

        #endregion

        #region Connect && Authenticate

        public async Task ConnectAndAuthenticateAsync(List<AbstractConnection> cnxs)
        {
            foreach (var cnx in cnxs)
            {
                try
                {
                    await cnx.ConnectAsync();
                }
                catch (Limilabs.Client.ServerException serverException)
                {
                    _logger.ErrorException($"Could not connect to host server! connection id {cnx.ConnectionId}", serverException);
                    throw;
                }
                try
                {
                    await cnx.AuthenticateAsync();
                }
                catch (Limilabs.Client.ServerException serverException)
                {
                    _logger.ErrorException($"Authentication failed for connection id {cnx.ConnectionId}", serverException);
                    throw;
                }
            }
        }
        
        #endregion

        #region Select Inbox

        public async Task<bool> SelectInboxAsync(AbstractConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            if (connection is ImapConnection cnxImap)
            {
                try
                {
                    await cnxImap.ImapConnectionObj.SelectInboxAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"Can not select inbox for connection id {cnxImap.ConnectionId}", ex);          
                }
            }
            else if (connection is Pop3Connection)
            {
                //No need to select inbox for pop3 connection...
            }
            else
            {
                throw new NotImplementedException("Cannot download emails for this type of connection!");
            }
            return false;
        }

        #endregion

        public void Enqueue(AbstractConnection cnx)
        {
            SetIsBusy(cnx, false);
            _connectionsPool.Enqueue(cnx);
        }

        public List<AbstractConnection> GetAll()
        {
            return _sharedConnectionDescriptor.ConnectionsList;
        }

        public AbstractConnection GetOneAvailable()
        {
            if( _connectionsPool.Count == 0)
                return null;

            if(_connectionsPool.IsEmpty)
                return null;

            if (_connectionsPool.TryDequeue(out var cnx))
            {
                SetIsBusy(cnx, true);
                return cnx;
            }

            return null;
        }

        private void SetIsBusy(AbstractConnection ac, bool isBusy)
        {
            lock (_lock)
            {
                ac.IsBusy = isBusy;
            }
        }
    }
}