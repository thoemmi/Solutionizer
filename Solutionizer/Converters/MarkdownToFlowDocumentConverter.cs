using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Navigation;
using Solutionizer.Helper;

namespace Solutionizer.Converters {
    public class MarkdownToFlowDocumentConverter : IValueConverter {
        private readonly Markdown _markdown = new Markdown();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = value as string;
            if (String.IsNullOrEmpty(text)) {
                return null;
            }

            text = _markdown.Transform(text);
            text = HtmlToXamlConverter.ConvertHtmlToXaml(text, true);
            var flowDocument = (FlowDocument)XamlReader.Parse(text);
            SubscribeToAllHyperlinks(flowDocument);
            return flowDocument;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }

        private void SubscribeToAllHyperlinks(FlowDocument flowDocument) {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks) {
                link.RequestNavigate += OnRequestNavigate;
            }
        }

        private static IEnumerable<DependencyObject> GetVisuals(DependencyObject root) {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>()) {
                yield return child;
                foreach (var descendants in GetVisuals(child)) {
                    yield return descendants;
                }
            }
        }

        private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}