using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Threading;
using Ninject.Extensions.Logging;

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            InitBasicApp(Bootstrap.Instance.Kernel);

            Logger.Info("Check if program is running...");
            if (CheckForRunningApp())
            {
                Logger.Info("An instance of this program is already running...");
                Current.Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            var logger = loggerFactory.GetCurrentClassLogger();

            logger.ErrorException("an unhandled error was caught but could not been handled", e.Exception);
        }
    }
}
