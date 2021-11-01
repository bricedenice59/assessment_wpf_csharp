using DeveloperTest.ViewModels;
using DeveloperTest.Views;
using Ninject;
using Ninject.Extensions.Logging.Log4net;
using Ninject.Planning.Bindings.Resolvers;

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

           Kernel.Load<Log4NetModule>();
           Kernel.Bind<ServerConnectionPropertiesViewModel>().ToSelf().InTransientScope();
           Kernel.Bind<ServerConnectionPropertiesView>().ToSelf().InSingletonScope();
        }
    }
}
