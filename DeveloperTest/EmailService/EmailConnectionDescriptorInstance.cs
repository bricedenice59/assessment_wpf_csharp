using System.Collections.Concurrent;
using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public class EmailConnectionDescriptorInstance : IEmailConnectionDescriptorInstance
    {
        private ConnectionDescriptor _cd;

        public List<AbstractConnection> ConnectionsList { get; set; }

        public void SetConnectionData(ConnectionDescriptor cd)
        {
            _cd = cd;
        }

        public ConnectionDescriptor GetConnectionData()
        {
            return _cd;
        }
    }
}