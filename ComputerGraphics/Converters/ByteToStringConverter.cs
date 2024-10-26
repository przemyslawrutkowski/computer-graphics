using System.Globalization;
using System.Windows.Data;

namespace ComputerGraphics.Converters
{
    public class ByteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value?.ToString()) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (byte.TryParse(value as string, out byte result))
            {
                return result;
            }
            return (byte)0;
        }
    }
}