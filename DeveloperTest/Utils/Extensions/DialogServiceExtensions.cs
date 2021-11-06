using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using MvvmDialogs;

namespace DeveloperTest.Utils.Extensions
{
    public static class DialogServiceExtensions
    {
        public static Task<bool?> ShowDialogAsync(this IDialogService dialogService, INotifyPropertyChanged ownerViewModel, IModalDialogViewModel viewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool?>();

            Application.Current.MainWindow.Dispatcher.Invoke(() =>
            {
                var result = dialogService.ShowDialog(ownerViewModel, viewModel);
                taskCompletionSource.SetResult(result);
            });
            
            return taskCompletionSource.Task;
        }
    }
}
