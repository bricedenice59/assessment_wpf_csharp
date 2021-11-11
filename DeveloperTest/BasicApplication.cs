using System.Threading;
using System.Windows;
using CommonServiceLocator;
using DeveloperTest.Utils;
using Ninject;
using Ninject.Extensions.Logging;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace DeveloperTest
{
    public class BasicApplication : Application
    {
        private const string AppGuid = "96f075aa-dead-4878-a73f-46ece7b10f06";

        protected ILogger Logger { get; private set; }

        protected void InitBasicApp(IKernel kernel)
        {
            log4net.Config.XmlConfigurator.Configure();
            // AddConnectionsToPool Ninject.
            var serviceLocator = new NinjectServiceLocator(kernel);
            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            // Get logger for current class.
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            Logger = loggerFactory.GetCurrentClassLogger();
        }
        
        protected bool CheckForRunningApp()
        {
            // Allow only one instance of the software to be run.
            var mutex = new Mutex(false, @"Global\" + AppGuid);
            return !mutex.WaitOne(0, false);
        }
    }
}