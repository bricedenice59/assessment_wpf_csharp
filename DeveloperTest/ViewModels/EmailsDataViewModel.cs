using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DeveloperTest.EmailService;
using DeveloperTest.MessageBus;
using DeveloperTest.Utils.Events;
using DeveloperTest.Utils.Extensions;
using DeveloperTest.Utils.WPF;
using DeveloperTest.ValueObjects;
using DeveloperTest.ViewModels.Popups;
using MvvmDialogs;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ViewModels
{
    public class EmailsDataViewModel : CommonViewModel
    {
        #region Fields

        private readonly IEmailConnectionUtils _connectionUtils;
        private readonly IEmailDownloadService _emailDownloadService;
        private readonly IDialogService _dialogService;
        private EmailObject _selectedItem;
        private string _currentStatus;
        private bool _showAnimationStatus;

        private readonly EventHandler<ScanEmailsStatusChangedEventArgs> _scanEmailsStatusChangedEventHandler;
        private readonly EventHandler<NewEmailDiscoveredEventArgs> _newEmailDiscoveredEventHandler;

        #endregion

        #region Properties

        public string CurrentStatus
        {
            get => _currentStatus;
            set
            {
                Set(() => CurrentStatus, ref _currentStatus, value);
            }
        }

        public bool ShowStatusAnimation
        {
            get => _showAnimationStatus;
            set
            {
                Set(() => ShowStatusAnimation, ref _showAnimationStatus, value);
            }
        }

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
                    //notify UI we have a busy download
                    MessengerInstance.Send(new EmailBodyDownloadedMessage(value, true));
                    Logger.Info($"Email body with id:{value.Uid} has not been downloaded yet");

                    //subscribe event to be later notified once the message is downloaded and then notify UI
                    value.OnEmailBodyDownloaded += OnEmailBodyDownloaded;

                    //let's force downloading it
                    DownloadOnDemand(value);
                }
                else
                {
                    MessengerInstance.Send(new EmailBodyDownloadedMessage(value, false));
                    Logger.Info($"Body already downloaded for id:{value.Uid}");
                }
            }
        }

        private void OnEmailBodyDownloaded(object sender, DownloadBodyFinishedEventArgs e)
        {
            //only update UI if the current selected item is still the same, otherwise ignore
            if(_selectedItem.Uid == e.Email.Uid)
                MessengerInstance.Send(new EmailBodyDownloadedMessage(e.Email, false));
            e.Email.OnEmailBodyDownloaded -= OnEmailBodyDownloaded;
        }

        #endregion

        #region Ctor

        public EmailsDataViewModel(ILogger logger,
            IDialogService dialogService,
            IEmailConnectionUtils connectionUtils,
            IEmailDownloadService emailDownloadService) : base(logger)
        {
            _connectionUtils = connectionUtils;
            _emailDownloadService = emailDownloadService;
            _dialogService = dialogService;

            EmailsList = new ObservableCollection<EmailObject>();
            CurrentStatus = null;
            ShowStatusAnimation = false;

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

        private async void CallbackOnScanEmailsStatusChanged(object o, ScanEmailsStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case ScanProgress.InProgress:
                    ShowStatusAnimation = true;
                    CurrentStatus = "Download in progress...";
                    return;
                case ScanProgress.Completed:
                    CurrentStatus = "Download completed ! :)";
                    ShowStatusAnimation = false;
                    _emailDownloadService.NewEmailDiscovered -= _newEmailDiscoveredEventHandler;
                    _emailDownloadService.ScanEmailsStatusChanged -= _scanEmailsStatusChangedEventHandler;

                    if (_emailDownloadService.ProcessedBodies.Count != _emailDownloadService.ProcessedBodies.Distinct().Count())
                    {
                        // Duplicates exist and that's an issue as this means some bodies have been downloaded more than once
                        var errorPopupViewModel = new ErrorPopupViewModel(Logger)
                        {
                            Message = $"Some bodies have been downloaded more than once! \r\n Found {_emailDownloadService.ProcessedBodies.Distinct().Count()} distinct bodies downloaded but {_emailDownloadService.ProcessedBodies.Count} processed!!"
                        };

                        await _dialogService.ShowDialogAsync(this, errorPopupViewModel);
                    }

            return;
            }
        }

        private void CallbackOnNewEmailDiscovered(object o, NewEmailDiscoveredEventArgs e)
        {
            EmailsList.Add(e?.Email);
        }

        #endregion

        #region Methods

        private Task DownloadOnDemand(EmailObject emailObj)
        {
            return Task.Run(async () =>
            {
                //allocate new connection
                var newConnection = _connectionUtils.CreateOneConnection(null);

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
                    Logger.ErrorException("Something went wrong when trying to create a new connection for downloading an item on demand.", e);
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