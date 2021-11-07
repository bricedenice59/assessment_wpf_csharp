using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public class EmailServiceSharedContext : IEmailServiceSharedContext
    {
        private List<AbstractConnection> AvailableConnections { get; set; }

        public EmailServiceSharedContext()
        {

        }

        public void Init(int nbConnections)
        {
            AvailableConnections = new List<AbstractConnection>(nbConnections);
        }

        public List<AbstractConnection> GetAvailableConnections()
        {
            return AvailableConnections;
        }
    }
}