using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MahApps.Metro.Controls;

namespace Solutionizer.Framework {
    public interface IFlyoutManager {
        void ShowFlyout(object viewModel);
    }

    public class FlyoutManager : ObservableCollection<Flyout>, IFlyoutManager {
        private readonly IComponentContext _container;

        public FlyoutManager(IComponentContext container) {
            _container = container;
        }

        public void ShowFlyout(object viewModel) {
            var viewTypeFromViewModelType = ViewLocator.GetViewTypeFromViewModelType(viewModel.GetType());
            var view = (FrameworkElement)_container.Resolve(viewTypeFromViewModelType);

            view.DataContext = viewModel;

            var flyout = view as Flyout;
            if (flyout == null) {
                flyout = new Flyout { Content = view };
            }

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