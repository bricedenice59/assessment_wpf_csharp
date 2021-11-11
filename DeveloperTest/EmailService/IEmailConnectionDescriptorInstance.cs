using System.Collections.Generic;
using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailConnectionDescriptorInstance
    {
        List<AbstractConnection> ConnectionsList { get; set; }
        void SetConnectionData(ConnectionDescriptor cd);
        ConnectionDescriptor GetConnectionData();
    }
}