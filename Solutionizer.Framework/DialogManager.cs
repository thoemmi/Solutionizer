using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Solutionizer.Framework {
    public interface IDialogManager {
        Task<TResult> ShowDialog<TResult>(DialogViewModel<TResult> viewModel);
        Task  ShowDialog(DialogViewModel viewModel);
    }

    public class DialogManager : IDialogManager {
        public async Task ShowDialog(DialogViewModel viewModel) {
            var view = ViewLocator.GetViewForViewModel(viewModel);

            var dialog = view as BaseMetroDialog;
            if (dialog == null) {
                throw new InvalidOperationException(String.Format("The view {0} belonging to view model {1} does not inherit from {2}", view.GetType(), viewModel.GetType(), typeof(BaseMetroDialog)));
            }

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            await firstMetroWindow.ShowMetroDialogAsync(dialog.Title, dialog);
            await viewModel.Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog);
        }

        public async Task<TResult> ShowDialog<TResult>(DialogViewModel<TResult> viewModel) {
            var view = ViewLocator.GetViewForViewModel(viewModel);

            var dialog = view as BaseMetroDialog;
            if (dialog == null) {
                throw new InvalidOperationException(String.Format("The view {0} belonging to view model {1} does not inherit from {2}", view.GetType(), viewModel.GetType(), typeof(BaseMetroDialog)));
            }

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            await firstMetroWindow.ShowMetroDialogAsync(dialog.Title, dialog);
            var result = await viewModel.Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog);

            return result;
        }
    }
}