using DeveloperTest.ValueObjects;

namespace DeveloperTest.MessageBus
{
    public class EmailBodyDownloadedMessage
    {
        public EmailObject EmailObj { get; set; }
        public bool IsBusy { get; set; }

        public EmailBodyDownloadedMessage(EmailObject emailObj, bool isBusy)
        {
            EmailObj = emailObj;
            IsBusy = isBusy;
        }
    }
}
