using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Ninject.Extensions.Logging;

namespace DeveloperTest.Utils.WPF
{
    public abstract class CommonViewModel : ViewModelBase
    {
        protected readonly ILogger Logger;

        private ICommand _onLoadCommand;

        public ICommand OnLoadCommand { get { return _onLoadCommand ?? (_onLoadCommand = new RelayCommand(async () => await ExecuteOnLoad())); } }

        protected static IMessenger ApplicationMessenger => Messenger.Default;

        /// <summary>
        /// Method executed on window load event.
        /// </summary>
        protected virtual async Task ExecuteOnLoad()
        {
        }

        public CommonViewModel(ILogger logger)
        {
            Logger = logger;
        }
    }
}
