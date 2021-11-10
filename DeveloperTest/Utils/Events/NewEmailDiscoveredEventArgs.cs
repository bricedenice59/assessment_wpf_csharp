using DeveloperTest.EmailService;
using DeveloperTest.ValueObjects;

namespace DeveloperTest.Utils.Events
{
    public class NewEmailDiscoveredEventArgs : System.EventArgs
    {
        public EmailObject Email { get; }


        public NewEmailDiscoveredEventArgs(EmailObject emailObj)
        {
            Email = emailObj;
        }
    }
}
