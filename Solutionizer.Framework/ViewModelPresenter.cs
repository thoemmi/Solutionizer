using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Autofac;

namespace Solutionizer.Framework {
    public class ViewModelPresenter : ContentControl {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (object), typeof (ViewModelPresenter),
                new PropertyMetadata(default(object), OnViewModelChanged));

        public object ViewModel {
            get { return GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (DesignerProperties.GetIsInDesignMode(d)) {
                return;
            }

            var self = (ViewModelPresenter)d;
            self.Content = null;

            if (e.NewValue == null) {
                return;
            }

            var viewType = ViewLocator.GetViewTypeFromViewModelType(e.NewValue.GetType());
            if (viewType == null) {
                throw new InvalidOperationException("No View found for ViewModel of type " + e.NewValue.GetType());
            }

            var view = BootstrapperBase.Container.Resolve(viewType);

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null) {
                frameworkElement.DataContext = e.NewValue;
            }

            self.Content = view;
        }
    }
}