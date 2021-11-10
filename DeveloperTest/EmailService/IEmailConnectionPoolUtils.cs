using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailConnectionPoolUtils
    {
        void Init(ConnectionDescriptor cd, int nbConnections);
        List<AbstractConnection> GetAll();
        AbstractConnection GetOneAvailable();
        void FreeBusy(AbstractConnection ac);
    }
}
