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
        Task SelectInboxAsync(AbstractConnection connection);
        List<AbstractConnection> GetAll();
        AbstractConnection GetOneAvailable();
        void FreeBusy(AbstractConnection ac);
    }
}
