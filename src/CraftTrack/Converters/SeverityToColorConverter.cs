using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CraftTrack.Models.Enums;

namespace CraftTrack.Converters;

public class SeverityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SeverityLevel severity)
        {
            var key = severity switch
            {
                SeverityLevel.Rush => "RushColor",
                SeverityLevel.High => "HighColor",
                SeverityLevel.Low => "LowColor",
                _ => null
            };

            if (key is not null && Application.Current.TryFindResource(key) is SolidColorBrush brush)
                return brush;
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
