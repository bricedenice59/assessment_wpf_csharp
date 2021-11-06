﻿using DeveloperTest.Utils.WPF.Components.Popups;
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
            Kernel.Bind<ServerConnectionPropertiesViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ServerConnectionPropertiesView>().ToSelf().InSingletonScope();
            Kernel.Bind<ErrorPopupViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ErrorPopupView>().ToSelf().InTransientScope();
        }
    }
}
