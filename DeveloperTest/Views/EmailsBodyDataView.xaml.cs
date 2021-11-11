using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CommonServiceLocator;
using DeveloperTest.MessageBus;
using DeveloperTest.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using Ninject.Extensions.Logging;

namespace DeveloperTest.Views
{
    /// <summary>
    /// Interaction logic for EmailsBodyDataView.xaml
    /// </summary>
    public partial class EmailsBodyDataView : UserControl
    {
        private readonly ILogger _logger;
        public EmailsBodyDataView()
        {
            InitializeComponent();
            Loaded += EmailsBodyDataView_Loaded;
            var loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
            _logger = loggerFactory.GetCurrentClassLogger();
        }

        private void EmailsBodyDataView_Loaded(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Register<LoadHtmlMessage>(this, m =>
            {
                try
                {
                    _webBrowser.LoadHtml(m.Html);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(
                        () => ((EmailsBodyDataViewModel) this.DataContext).HasWebviewRenderingError = true);

                    _logger?.ErrorException("Exception caught in webbrowser", ex);
                }
            });
        }
    }
}
