namespace DeveloperTest.Utils.WPF.Components.Popups
{
    public interface IClosing
    {
        /// <summary>
        /// Executes when window is closing
        /// </summary>
        /// <returns>Returns true if the windows should be closed by the caller</returns>
        bool OnClosing();
    }
}
