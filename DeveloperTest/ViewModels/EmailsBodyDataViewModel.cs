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

        #endregion

        #region Ctor

        public EmailsBodyDataViewModel(ILogger logger) : base(logger)
        {
            IsBodyAvailable = false;
            ShowThisView = false;

            ApplicationMessenger.Register<EmailBodyDownloadedMessage>(this, m =>
            {
                IsBodyAvailable = !m.IsBusy;
                if (m.EmailObj.IsBodyDownloaded)
                {
                    ApplicationMessenger.Send(new LoadHtmlMessage(m.EmailObj.Body));
                }

                if(!_showThisView)
                    ShowThisView = true;
            });
        }

        #endregion

        #region Methods


        #endregion
    }
}