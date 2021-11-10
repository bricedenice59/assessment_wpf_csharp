using DeveloperTest.EmailService;

namespace DeveloperTest.Utils.Events
{
    public class ScanEmailsStatusChangedEventArgs : System.EventArgs
    {
        public ScanProgress Status { get; }


        public ScanEmailsStatusChangedEventArgs(ScanProgress status)
        {
            Status = status;
        }
    }
}
