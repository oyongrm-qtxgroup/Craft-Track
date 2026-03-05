using System.Globalization;
using System.Windows.Data;

namespace CraftTrack.Converters;

/// <summary>
/// MultiValueConverter that returns true if both values are equal strings (case-insensitive).
/// Used for preset card selection highlighting.
/// </summary>
public class StringEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        var a = values[0]?.ToString();
        var b = values[1]?.ToString();
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
