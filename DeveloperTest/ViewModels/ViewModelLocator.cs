using CommonServiceLocator;
using DeveloperTest.ViewModels.Popups;

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

        public EmailsDataViewModel EmailsDataViewModel => ServiceLocator.Current.GetInstance<EmailsDataViewModel>();
        public EmailsBodyDataViewModel EmailsBodyDataViewModel => ServiceLocator.Current.GetInstance<EmailsBodyDataViewModel>();
        public ServerConnectionPropertiesViewModel ServerConnectionProperties => ServiceLocator.Current.GetInstance<ServerConnectionPropertiesViewModel>();
        public ErrorPopupViewModel ErrorPopup => ServiceLocator.Current.GetInstance<ErrorPopupViewModel>();
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}