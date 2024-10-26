using System.Globalization;
using System.Windows.Data;

namespace ComputerGraphics.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value?.ToString()) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value as string, NumberStyles.Any, culture, out double result))
            {
                return result;
            }
            return 0.0;
        }
    }
}
