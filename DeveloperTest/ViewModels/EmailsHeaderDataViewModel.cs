using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DeveloperTest.EmailService;
using DeveloperTest.MessageBus;
using DeveloperTest.Utils.Events;
using DeveloperTest.Utils.WPF;
using DeveloperTest.ValueObjects;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ViewModels
{
    public class EmailsHeaderDataViewModel : CommonViewModel
    {
        #region Fields

        private readonly EventHandler<NewEmailDiscoveredEventArgs> _newEmailDiscoveredEventHandler;
        private readonly EventHandler<ScanEmailsStatusChangedEventArgs> _scanEmailsStatusChangedEventHandler;
        private readonly EmailDownloadService _emailDownloadService;
        #endregion
        public ObservableCollection<EmailObject> EmailsList { get; set; }

        public EmailsHeaderDataViewModel(ILogger logger) : base(logger)
        {
            _emailDownloadService = new EmailDownloadService();
            EmailsList = new ObservableCollection<EmailObject>();

            _newEmailDiscoveredEventHandler = EventHandlerHelper.SafeEventHandler<NewEmailDiscoveredEventArgs>(CallbackOnNewEmailDiscovered);
            _scanEmailsStatusChangedEventHandler = EventHandlerHelper.SafeEventHandler<ScanEmailsStatusChangedEventArgs>(CallbackOnScanEmailsStatusChanged);

            ApplicationMessenger.Register<StartScanEmailMessage>(this, m =>
            {
                _emailDownloadService.NewEmailDiscovered += _newEmailDiscoveredEventHandler;
                _emailDownloadService.ScanEmailsStatusChanged += _scanEmailsStatusChangedEventHandler;
                _emailDownloadService.DownloadEmails();
            });
        }

        private void CallbackOnScanEmailsStatusChanged(object o, ScanEmailsStatusChangedEventArgs e)
        {
            if (e.Status == ScanProgress.InProgress)
            {
                return;
            }
            if (e.Status == ScanProgress.Completed)
            {
                _emailDownloadService.NewEmailDiscovered -= _newEmailDiscoveredEventHandler;
                _emailDownloadService.ScanEmailsStatusChanged -= _scanEmailsStatusChangedEventHandler;
            }
        }

        private void CallbackOnNewEmailDiscovered(object o, NewEmailDiscoveredEventArgs e)
        {
            EmailsList.Add(e?.Email);
        }
    }
}