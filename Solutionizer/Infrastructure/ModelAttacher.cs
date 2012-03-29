using System;
using System.Windows;

namespace Solutionizer.Infrastructure {
    public static class ModelAttacher {
        public static readonly DependencyProperty AttachModelProperty =
            DependencyProperty.RegisterAttached("AttachModel", typeof (Type), typeof (ModelAttacher),
                                                new PropertyMetadata(null, AttachModelCallback));

        private static void AttachModelCallback(DependencyObject source, DependencyPropertyChangedEventArgs args) {
            var modelType = args.NewValue as Type;
            var view = source as FrameworkElement;
            if (modelType == null || view == null) {
                return;
            }

            try {
                var model = Activator.CreateInstance(modelType);
                view.DataContext = model;
            } catch (Exception ex) {
                throw new InvalidOperationException(string.Format("Cannot create instance of model type: {0}", modelType), ex);
            }
        }

        public static Type GetAttachModel(UIElement element) {
            return (Type)element.GetValue(AttachModelProperty);
        }

        public static void SetAttachModel(UIElement element, Type value) {
            element.SetValue(AttachModelProperty, value);
        }
    }
}