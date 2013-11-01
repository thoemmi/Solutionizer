using System.Windows;
using Autofac;
using NLog;

namespace Solutionizer.Framework {
    public class WindowManager {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext _container;

        public WindowManager(IComponentContext container) {
            _container = container;
        }

        public void ShowWindow<TViewModel>() {
            var viewModel = _container.Resolve<TViewModel>();

            var view = (Window)ViewLocator.GetViewForViewModel(viewModel);

            view.Show();
        }
    }
}