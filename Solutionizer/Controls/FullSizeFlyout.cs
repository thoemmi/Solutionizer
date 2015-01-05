using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Solutionizer.Converters;

namespace Solutionizer.Controls {
    public class FullSizeFlyout : Flyout {
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var widthBinding = new Binding {
                Path = new PropertyPath("ActualWidth"),
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) {
                    AncestorType = typeof(MetroWindow)
                },
                Converter = new AddDoubleConverter(),
                ConverterParameter = -150
            };
            SetBinding(WidthProperty, widthBinding);
        }
    }
}