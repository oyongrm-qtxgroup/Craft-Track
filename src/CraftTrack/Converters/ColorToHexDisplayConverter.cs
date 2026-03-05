using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CraftTrack.Converters;

/// <summary>
/// Converts a WPF <see cref="Color"/> to a 6-character hex display string like "#7C3AED".
/// Strips the alpha channel for display.
/// </summary>
public class ColorToHexDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color c)
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        return "#000000";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
