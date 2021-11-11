using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommonServiceLocator;
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
        private readonly IEmailConnectionUtils _connectionUtils;
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

                Logger.Info($"Downloading body on demand for id:{value.Uid}");

                if (value.IsBodyBeingDownloaded)
                {
                    Logger.Info($"Email body with id:{value.Uid} is being downloaded, body will be soon available...");
                    return;
                }

                //if body not downloaded yet, then request to download it.
                if (!value.IsBodyDownloaded)
                {
                    Logger.Info($"Email body with id:{value.Uid} has not been downloaded yet");
                    DownloadOnDemandAsync(value);
                }
                else Logger.Info($"Body already downloaded for id:{value.Uid}");
            }
        }

        #endregion

        #region Ctor

        public EmailsHeaderDataViewModel(ILogger logger) : base(logger)
        {
            _connectionUtils = ServiceLocator.Current.GetInstance<IEmailConnectionUtils>();
            _emailDownloadService = new EmailDownloadService();
            EmailsList = new ObservableCollection<EmailObject>();

            _newEmailDiscoveredEventHandler = EventHandlerHelper.SafeEventHandler<NewEmailDiscoveredEventArgs>(CallbackOnNewEmailDiscovered);
            _scanEmailsStatusChangedEventHandler = EventHandlerHelper.SafeEventHandler<ScanEmailsStatusChangedEventArgs>(CallbackOnScanEmailsStatusChanged);

            ApplicationMessenger.Register<StartScanEmailMessage>(this, async m =>
            {
                _emailDownloadService.NewEmailDiscovered += _newEmailDiscoveredEventHandler;
                _emailDownloadService.ScanEmailsStatusChanged += _scanEmailsStatusChangedEventHandler;
                await _emailDownloadService.DownloadEmails();
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

        #region Methods

        private void DownloadOnDemandAsync(EmailObject emailObj)
        {
            Task.Run(async () =>
            {
                //allocate new connection
                var newConnection = _connectionUtils.CreateOneConnection();

                //then connect and authenticate
                bool connectAndAuthenticateSuccess = true;
                try
                {
                    await newConnection.ConnectAsync();
                    await newConnection.AuthenticateAsync();
                    await _connectionUtils.SelectInboxAsync(newConnection);
                }
                catch (Exception e)
                {
                    connectAndAuthenticateSuccess = false;
                    Logger.ErrorException("Something went wrong when trying to create a new connection for item download on demand.", e);
                }

                //download body
                if (connectAndAuthenticateSuccess)
                    await _emailDownloadService.DownloadBody(emailObj, newConnection);

                //close and dispose connection
                await newConnection.DisconnectAsync();
            });
        }

        #endregion
    }
}