using System;
using System.Globalization;
using System.Windows.Data;

namespace Solutionizer.Converters {
    public class AddDoubleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var doubleValue = System.Convert.ToDouble(value);
            var summand = System.Convert.ToDouble(parameter);
            return doubleValue + summand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var doubleValue = System.Convert.ToDouble(value);
            var summand = System.Convert.ToDouble(parameter);
            return doubleValue - summand;
        }
    }
}