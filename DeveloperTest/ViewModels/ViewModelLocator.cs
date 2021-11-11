/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:IWDeviceRecovery"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using DeveloperTest.ViewModels.Popups;
using GalaSoft.MvvmLight.Messaging;

namespace DeveloperTest.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
        }

        public EmailsHeaderDataViewModel EmailsHeaderDataViewModel => ServiceLocator.Current.GetInstance<EmailsHeaderDataViewModel>();
        public EmailsBodyDataViewModel EmailsBodyDataViewModel => ServiceLocator.Current.GetInstance<EmailsBodyDataViewModel>();
        public ServerConnectionPropertiesViewModel ServerConnectionProperties => ServiceLocator.Current.GetInstance<ServerConnectionPropertiesViewModel>();
        public ErrorPopupViewModel ErrorPopup => ServiceLocator.Current.GetInstance<ErrorPopupViewModel>();
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}