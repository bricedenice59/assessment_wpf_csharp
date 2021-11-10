using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DeveloperTest.EmailService;
using DeveloperTest.MessageBus;
using DeveloperTest.Utils.Events;
using DeveloperTest.Utils.WPF;
using DeveloperTest.ValueObjects;
using Limilabs.Client.IMAP;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ViewModels
{
    public class EmailsHeaderDataViewModel : CommonViewModel
    {
        #region Fields

        private readonly EventHandler<NewEmailDiscoveredEventArgs> _newEmailDiscoveredEventHandler;
        private readonly EventHandler<ScanEmailsStatusChangedEventArgs> _scanEmailsStatusChangedEventHandler;
        private readonly EmailDownloadService _emailDownloadService;
        private EmailObject _selectedItem;
        #endregion

        #region Properties

        public ObservableCollection<EmailObject> EmailsList { get; }

        public EmailObject SelectedItem
        {
            get => _selectedItem;
            set
            {
                //avoid going further with multiple clicks when the item clicked in datagrid is already selected
                if (value == _selectedItem)
                    return;

                Set(() => SelectedItem, ref _selectedItem, value);

                //if body not downloaded yet, then request to download it.
                if (!value.IsBodyDownloaded)
                {
#if DEBUG
                    Logger.Info($"Email body with id:{value.Uid} has not been downloaded yet");
#endif
                }
            }
        }

        #endregion

        #region Ctor

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

        #endregion

        #region Events implementation

        private void CallbackOnScanEmailsStatusChanged(object o, ScanEmailsStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case ScanProgress.InProgress:
                    return;
                case ScanProgress.Completed:
                    _emailDownloadService.NewEmailDiscovered -= _newEmailDiscoveredEventHandler;
                    _emailDownloadService.ScanEmailsStatusChanged -= _scanEmailsStatusChangedEventHandler;
                    break;
            }
        }

        private void CallbackOnNewEmailDiscovered(object o, NewEmailDiscoveredEventArgs e)
        {
            EmailsList.Add(e?.Email);
        }

        #endregion
    }
}