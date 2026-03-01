using System.Globalization;
using System.Windows.Data;

namespace CraftTrack.Converters;

public class CurrencyFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
            return $"₱{amount:N2}";
        return "₱0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            str = str.Replace("₱", "").Replace(",", "").Trim();
            if (decimal.TryParse(str, out var result))
                return result;
        }
        return 0m;
    }
}
