using System.Windows;

namespace DeveloperTest.Utils.WPF.Components.Popups
{
    public class DialogWindow : WindowBase
    {
        #region PopupWindowResult

        /// <summary>
        /// PopupWindowResult Dependency Property
        /// </summary>
        public static readonly DependencyProperty PopupWindowResultProperty =
            DependencyProperty.Register("PopupWindowResult", typeof(bool?), typeof(DialogWindow),
                new FrameworkPropertyMetadata(null, OnPopupWindowResultChanged));

        /// <summary>
        /// Gets or sets the PopupWindowResult property. This dependency property 
        /// indicates modal popup result.
        /// </summary>
        public bool? PopupWindowResult
        {
            get { return (bool?)GetValue(PopupWindowResultProperty); }
            set { SetValue(PopupWindowResultProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PopupWindowResult property.
        /// </summary>
        private static void OnPopupWindowResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogWindow target = (DialogWindow)d;
            bool? oldPopupWindowResult = (bool?)e.OldValue;
            bool? newPopupWindowResult = target.PopupWindowResult;
            target.OnPopupWindowResultChanged(oldPopupWindowResult, newPopupWindowResult);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the PopupWindowResult property.
        /// </summary>
        protected virtual void OnPopupWindowResultChanged(bool? oldPopupWindowResult, bool? newPopupWindowResult)
        {
            if (IsInitialized && IsLoaded && newPopupWindowResult.HasValue && !DialogResult.HasValue)
                DialogResult = newPopupWindowResult;
        }

        #endregion
    }
}
