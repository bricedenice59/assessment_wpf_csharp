using DeveloperTest.ValueObjects;

namespace DeveloperTest.MessageBus
{
    public class RequestDownloadOnDemandMessage : System.EventArgs
    {
        public EmailObject EmailObj { get; set; }

        public RequestDownloadOnDemandMessage(EmailObject emailObj)
        {
            EmailObj = emailObj;
        }
    }
}
