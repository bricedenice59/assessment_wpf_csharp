using DeveloperTest.MessageBus;
using DeveloperTest.Utils.WPF;
using Ninject.Extensions.Logging;

namespace DeveloperTest.ViewModels
{
    public class EmailsBodyDataViewModel : CommonViewModel
    {
        #region Fields

        private bool _isBodyAvailable;
        private bool _showThisView;
        private bool _hasWebviewRenderingError;
        #endregion

        #region Properties

        public bool ShowThisView
        {
            get => _showThisView;
            set
            {
                Set(() => ShowThisView, ref _showThisView, value);
            }
        }

        public bool IsBodyAvailable
        {
            get => _isBodyAvailable;
            set
            {
                Set(() => IsBodyAvailable, ref _isBodyAvailable, value);
            }
        }

        public bool HasWebviewRenderingError
        {
            get => _hasWebviewRenderingError;
            set
            {
                Set(() => HasWebviewRenderingError, ref _hasWebviewRenderingError, value);
                if(value)
                    ShowThisView = false;
            }
        }

        #endregion

        #region Ctor

        public EmailsBodyDataViewModel(ILogger logger) : base(logger)
        {
            IsBodyAvailable = false;
            ShowThisView = false;
            HasWebviewRenderingError = false;

            ApplicationMessenger.Register<EmailBodyDownloadedMessage>(this, m =>
            {
                HasWebviewRenderingError = false;
                IsBodyAvailable = !m.IsBusy;
                if (!_showThisView)
                    ShowThisView = true;

                if (m.EmailObj.IsBodyDownloaded)
                {
                    ApplicationMessenger.Send(new LoadHtmlMessage(m.EmailObj.Body));
                }
            });
        }

        #endregion

        #region Methods


        #endregion
    }
}