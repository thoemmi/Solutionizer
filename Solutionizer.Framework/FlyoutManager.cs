using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace Solutionizer.Framework {
    public interface IFlyoutManager {
        void ShowFlyout(object viewModel);
    }

    public class FlyoutManager : ObservableCollection<Flyout>, IFlyoutManager {
        public void ShowFlyout(object viewModel) {
            var view = (FrameworkElement)ViewLocator.GetViewForViewModel(viewModel);

            var flyout = view as Flyout ?? new Flyout { Content = view };

            EventHandler handler = null;
            handler = (sender, args) => {
                flyout.IsOpenChanged -= handler;
                DelayedRemove(flyout);
            };
            flyout.IsOpenChanged += handler;

            Add(flyout);

            flyout.IsOpen = true;
        }

        private async void DelayedRemove(Flyout flyout) {
            if (flyout.IsOpen == false) {
                await Task.Delay(1000);
                Remove(flyout);
            }
        }
    }
}