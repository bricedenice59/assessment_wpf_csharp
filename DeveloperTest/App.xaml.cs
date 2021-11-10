using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DeveloperTest.Utils;
using GalaSoft.MvvmLight.Threading;

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
            InitBasicApp(Bootstrap.Instance.Kernel);

            Logger.Info("Check if program is running...");
            if (CheckForRunningApp())
            {
                Logger.Info("An instance of this program is already running...");
                Current.Shutdown();
            }
        }
    }
}
