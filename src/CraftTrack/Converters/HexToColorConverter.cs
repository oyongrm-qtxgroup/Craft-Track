using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CraftTrack.Converters;

/// <summary>
/// Converts a hex color string (e.g. "#7C3AED") to a <see cref="Color"/>.
/// </summary>
public class HexToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch { }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color c)
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        return "#808080";
    }
}
