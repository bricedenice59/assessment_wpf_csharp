using System;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CommonServiceLocator;
using DeveloperTest.MessageBus;
using DeveloperTest.Utils.WPF;
using GalaSoft.MvvmLight.Messaging;
using Ninject.Extensions.Logging;

namespace DeveloperTest.Views
{
    /// <summary>
    /// Interaction logic for EmailsBodyDataView.xaml
    /// </summary>
    public partial class EmailsBodyDataView : UserControl
    {
        private object _dataContext;
        private ILogger _logger;
        public EmailsBodyDataView()
        {
            InitializeComponent();
            Loaded += EmailsBodyDataView_Loaded;

            _webBrowser.IsBrowserInitializedChanged += _webBrowser_IsBrowserInitializedChanged;

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
                    _logger?.ErrorException("Exception caught in webbrowser", ex);
                }
            });
        }

        private void _webBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           //just here for testing
        }
    }
}
