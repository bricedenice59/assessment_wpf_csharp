using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Ninject.Extensions.Logging;

namespace DeveloperTest
{
    public abstract class CommonViewModel : ViewModelBase
    {
        protected readonly ILogger Logger;

        private ICommand _onLoadCommand;

        public ICommand OnLoadCommand { get { return _onLoadCommand ?? (_onLoadCommand = new RelayCommand(async () => await ExecuteOnLoad())); } }

        #pragma warning disable 1998
        /// <summary>
        /// Method executed on window load event.
        /// </summary>
        protected virtual async Task ExecuteOnLoad()
        {
        }
        #pragma warning restore 1998

        public CommonViewModel(ILogger logger)
        {
            Logger = logger;
        }
    }
}
