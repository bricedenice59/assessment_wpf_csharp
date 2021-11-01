using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DeveloperTest.Utils;

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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
