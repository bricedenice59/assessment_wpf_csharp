using System.ComponentModel;
using System.Windows;
using DeveloperTest.Utils.WPF.Components.Popups;

namespace DeveloperTest.Utils.WPF.Components
{
    public abstract class WindowBase : System.Windows.Window
    {
        protected override void OnClosing(CancelEventArgs e)
        {
            var closing = DataContext as IClosing;
            if (closing != null)
            {
                e.Cancel = !closing.OnClosing();
            }

            if(closing == null)
                base.OnClosing(e);
        }
    }
}
