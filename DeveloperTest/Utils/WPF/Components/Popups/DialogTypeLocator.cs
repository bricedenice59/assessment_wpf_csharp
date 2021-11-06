using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MvvmDialogs.DialogTypeLocators;

namespace DeveloperTest.Utils.WPF.Components.Popups
{
    public class DialogTypeLocator : IDialogTypeLocator
    {
        internal static readonly Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();

        private static string GetAssemblyFullName(Type viewModelType)
        {
            return viewModelType.Assembly.FullName;
        }

        /// <summary>
        /// Locates the dialog type representing the specified view model in a user interface.
        /// </summary>
        /// <param name="viewModel">The view model to find the dialog type for.</param>
        /// <returns>
        /// The dialog type representing the specified view model in a user interface.
        /// </returns>
        public Type Locate(INotifyPropertyChanged viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            Type viewModelType = viewModel.GetType();

            Type dialogType;
            bool viewHasBeenResolved = false;

            if (Cache.TryGetValue(viewModelType, out dialogType))
            {
                return dialogType;
            }

            var dialogFullName = GetDialogFullName(viewModelType);
            if(dialogFullName != null)
                dialogType = Type.GetType(dialogFullName);

            viewHasBeenResolved = dialogType != null;
            if (dialogType == null)
            {
                string dialogName = viewModelType.Name
                    .Replace(".ViewModels.", ".Views.")
                    .Replace(".ViewModel.", ".View.")
                    .Replace("ViewModel", "View");

                var viewFromEntryAssembly = Assembly.GetEntryAssembly()?.ExportedTypes.FirstOrDefault(x=>x.Name == dialogName);
                if (viewFromEntryAssembly != null)
                {
                    string dialogAssemblyName = GetAssemblyFullName(viewFromEntryAssembly);

                    var fullDialogName = $"{viewFromEntryAssembly.FullName}, {dialogAssemblyName}";
                    dialogType = Type.GetType(fullDialogName);
                    viewHasBeenResolved = dialogType != null;
                }
            }
            if(!viewHasBeenResolved)
                throw new TypeLoadException($"Dialog with full name '{dialogFullName}' is missing.");

            Cache.Add(viewModelType, dialogType);

            return dialogType;
        }

        private string GetDialogFullName(Type viewModelType)
        {
            if (viewModelType.FullName != null)
            {
                string dialogName = viewModelType.FullName
                    .Replace(".ViewModels.", ".Views.")
                    .Replace(".ViewModel.", ".View.")
                    .Replace("ViewModel", "View");

                string dialogAssemblyName = GetAssemblyFullName(viewModelType);

                return $"{dialogName}, {dialogAssemblyName}";
            }

            return null;
        }
    }
}
