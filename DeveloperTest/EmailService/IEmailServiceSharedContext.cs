using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailServiceSharedContext
    {
        void Init(ConnectionDescriptor cd, int nbConnections);
        List<AbstractConnection> GetAllConnections();
        AbstractConnection GetOneAvailableConnection();
        void FreeBusyConnection(AbstractConnection ac);
    }
}
