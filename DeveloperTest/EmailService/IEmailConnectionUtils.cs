using System.Collections.Generic;
using System.Threading.Tasks;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailConnectionUtils
    {
        List<AbstractConnection> CreateConnections();
        AbstractConnection CreateOneConnection(int? idConnection);
        Task ConnectAndAuthenticateAsync(List<AbstractConnection> cnxs);
        Task DisconnectAsync(List<AbstractConnection> cnxs);
        Task<bool> SelectInboxAsync(AbstractConnection connection);
        void Enqueue(AbstractConnection cnx);
        List<AbstractConnection> GetAll();
        AbstractConnection GetOneAvailable();
    }
}
