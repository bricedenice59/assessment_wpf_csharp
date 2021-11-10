using DeveloperTest.ConnectionService;

namespace DeveloperTest.EmailService
{
    public interface IEmailConnectionDescriptorInstance
    {
        void SetConnectionData(ConnectionDescriptor cd);
        ConnectionDescriptor GetConnectionData();
    }
}