using System;
using System.Reflection;
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

            var viewTypeFromViewModelType = ViewLocator.GetViewTypeFromViewModelType(viewModel.GetType());
            var view = _container.Resolve(viewTypeFromViewModelType) as Window;
            if (view == null) {
                _log.Error("WindowManager.ShowWindow called with type parameter which does not inherit from Window");
                throw new ArgumentException("WindowManager.ShowWindow called with type parameter which does not inherit from Window");
            }

            view.DataContext = viewModel;

            //var onLoadedHandler = viewModel as IOnLoadedHandler;
            //if (onLoadedHandler != null) {
            //    RoutedEventHandler handler = null;
            //    handler = (sender, args) => {
            //        view.Loaded -= handler;
            //        onLoadedHandler.OnViewLoaded();
            //    };
            //    view.Loaded += handler;
            //}

            InitializeComponent(view);

            view.Show();
        }

        public static void InitializeComponent(object element) {
            var method = element.GetType().GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) {
                return;
            }
            method.Invoke(element, null);
        }
    }
}