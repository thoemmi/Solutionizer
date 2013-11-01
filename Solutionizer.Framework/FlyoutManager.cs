using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace Solutionizer.Framework {
    public interface IFlyoutManager {
        Task ShowFlyout(object viewModel);
    }

    public class FlyoutManager : ObservableCollection<Flyout>, IFlyoutManager {
        public Task ShowFlyout(object viewModel) {
            var view = (FrameworkElement)ViewLocator.GetViewForViewModel(viewModel);

            var flyout = view as Flyout ?? new Flyout { Content = view };
            flyout.IsOpen = true;

            var tcs = new TaskCompletionSource<int>();

            EventHandler handler = null;
            handler = (sender, args) => {
                tcs.SetResult(0);
                flyout.IsOpenChanged -= handler;
                DelayedRemove(flyout);
            };
            flyout.IsOpenChanged += handler;

            Add(flyout);

            return tcs.Task;
        }

        private async void DelayedRemove(Flyout flyout) {
            if (flyout.IsOpen == false) {
                await Task.Delay(1000);
                Remove(flyout);
            }
        }
    }
}