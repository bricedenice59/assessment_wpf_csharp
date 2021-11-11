using DeveloperTest.ValueObjects;

namespace DeveloperTest.Utils.Events
{
    public class DownloadBodyFinishedEventArgs : System.EventArgs
    {
        public EmailObject Email { get; }

        public DownloadBodyFinishedEventArgs(EmailObject emailObj)
        {
            Email = emailObj;
        }
    }
}
