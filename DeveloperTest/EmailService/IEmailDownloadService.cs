using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DeveloperTest.ConnectionService;
using DeveloperTest.Utils.Events;
using DeveloperTest.ValueObjects;

namespace DeveloperTest.EmailService
{
    public interface IEmailDownloadService
    {
        event EventHandler<ScanEmailsStatusChangedEventArgs> ScanEmailsStatusChanged;
        event EventHandler<NewEmailDiscoveredEventArgs> NewEmailDiscovered;
        Task DownloadEmails();
        Task DownloadBody(EmailObject emailObj, AbstractConnection connection);
        ConcurrentBag<string> ProcessedBodies { get; set; }
    }
}
