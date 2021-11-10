using DeveloperTest.EmailService;
using DeveloperTest.Utils.WPF.Components.Popups;
using DeveloperTest.ViewModels;
using DeveloperTest.ViewModels.Popups;
using DeveloperTest.Views;
using DeveloperTest.Views.Popups;
using Ninject;
using Ninject.Extensions.Logging.Log4net;
using Ninject.Planning.Bindings.Resolvers;
using MvvmDialogs;

namespace DeveloperTest
{
    public class Bootstrap
    {
        private static Bootstrap _instance;

        public static Bootstrap Instance => _instance ?? (_instance = new Bootstrap());

        public IKernel Kernel { get; private set; }

        private Bootstrap()
        {
            var settings = new NinjectSettings { LoadExtensions = false };
            Kernel = new StandardKernel(settings);
            Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

            Kernel.Bind<IDialogService>().ToMethod(context => new DialogService(null, new DialogTypeLocator()));
            Kernel.Load<Log4NetModule>();
            InjectViews();
            InjectViewModels();
            
            Kernel.Bind<IEmailConnectionPoolUtils>().To<EmailConnectionPoolUtils>().InSingletonScope();

            //we want this class to have only one instance in the program life 
            Kernel.Bind<IEmailConnectionDescriptorInstance>().To<EmailConnectionDescriptorInstance>().InSingletonScope();
        }

        private void InjectViews()
        {
            Kernel.Bind<EmailsHeaderDataView>().ToSelf().InTransientScope();
            Kernel.Bind<ServerConnectionPropertiesView>().ToSelf().InTransientScope();
            Kernel.Bind<ErrorPopupView>().ToSelf().InTransientScope();
        }

        private void InjectViewModels()
        {
            Kernel.Bind<EmailsHeaderDataViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ServerConnectionPropertiesViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ErrorPopupViewModel>().ToSelf().InTransientScope();
        }
    }
}
