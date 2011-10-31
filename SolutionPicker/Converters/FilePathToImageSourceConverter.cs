using System;
using System.Globalization;
using System.Windows.Data;
using SolutionPicker.Helper;

namespace SolutionPicker.Converters {
    public class FilePathToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var path = (string) value;
            return Icons.GetImageForFile(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}