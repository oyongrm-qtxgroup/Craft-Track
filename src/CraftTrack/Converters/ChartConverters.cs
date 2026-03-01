using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CraftTrack.Models.Enums;

namespace CraftTrack.Converters;

/// <summary>
/// Converts ExpenseCategory enum to the matching ChartBar brush from theme resources.
/// </summary>
public class ExpenseCategoryColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ExpenseCategory category)
            return Brushes.Gray;

        var key = category switch
        {
            ExpenseCategory.Materials => "ChartBarMaterials",
            ExpenseCategory.Tools => "ChartBarTools",
            ExpenseCategory.Packaging => "ChartBarPackaging",
            ExpenseCategory.Utilities => "ChartBarUtilities",
            _ => "ChartBarOther"
        };

        return Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a double percentage (0-100) to a GridLength star value for chart bar width.
/// </summary>
public class PercentageToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double pct && pct > 0)
            return new GridLength(pct, GridUnitType.Star);
        return new GridLength(0.01, GridUnitType.Star);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the remaining portion (100 - value) as a GridLength star for the blank space.
/// </summary>
public class PercentageRemainderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double pct && pct > 0 && pct < 100)
            return new GridLength(100 - pct, GridUnitType.Star);
        if (value is double p && p >= 100)
            return new GridLength(0.01, GridUnitType.Star);
        return new GridLength(99.99, GridUnitType.Star);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
