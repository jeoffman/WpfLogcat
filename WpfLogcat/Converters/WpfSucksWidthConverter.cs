using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfLogcat.Converters
{
    public class WpfSucksWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int doubleValue = (int)value;
            return doubleValue == double.NaN ? 0 : doubleValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
