using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
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
            return XamlReader.Parse(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}