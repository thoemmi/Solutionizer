using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Solutionizer.Helper;

namespace Solutionizer.Converters {
    public class FilePathToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var path = (string) value;
            return Icons.GetImageForFile(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}