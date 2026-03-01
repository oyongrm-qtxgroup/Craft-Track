using System.Globalization;
using System.Windows.Data;

namespace CraftTrack.Converters;

/// <summary>
/// Inverts a boolean value. Useful for IsReadOnly bindings where you want
/// the opposite of a boolean property.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}
