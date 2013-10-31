using System;
using System.Reflection;
using System.Windows;
using Autofac;

namespace Solutionizer.Framework {
    public static class ViewLocator {
        public static Func<Type, Type> GetViewTypeFromViewModelType;
        public static Func<string, string> GetViewTypeNameFromViewModelTypeName;

        static ViewLocator() {
            GetViewTypeNameFromViewModelTypeName = viewModeltypeName => viewModeltypeName.Replace("ViewModel", "View");
            GetViewTypeFromViewModelType = type => {
                var viewModelTypeName = type.FullName;
                var viewTypeName = GetViewTypeNameFromViewModelTypeName(viewModelTypeName);
                var viewType = type.Assembly.GetType(viewTypeName);
                return viewType;
            };
        }

        public static object GetViewForViewModel(object viewModel) {
            var viewType = GetViewTypeFromViewModelType(viewModel.GetType());
            if (viewType == null) {
                throw new InvalidOperationException("No View found for ViewModel of type " + viewModel.GetType());
            }

            var view = BootstrapperBase.Container.Resolve(viewType);

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null) {
                frameworkElement.DataContext = viewModel;
            }

            InitializeComponent(view);

            return view;
        }

        private static void InitializeComponent(object element) {
            var method = element.GetType().GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) {
                return;
            }
            method.Invoke(element, null);
        }
    }
}