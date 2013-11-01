using System.Windows;
using Autofac;

namespace Solutionizer.Framework {
    public class WindowManager {
        private readonly IComponentContext _container;

        public WindowManager(IComponentContext container) {
            _container = container;
        }

        public void ShowWindow<TViewModel>() {
            var viewModel = _container.Resolve<TViewModel>();

            var view = (Window) ViewLocator.GetViewForViewModel(viewModel);

            view.Show();
        }
    }
}