using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailServiceSharedContext
    {
        void Init(int nbConnections);
        List<AbstractConnection> GetAvailableConnections();
    }
}
