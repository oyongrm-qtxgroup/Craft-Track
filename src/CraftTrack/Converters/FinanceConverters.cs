using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CraftTrack.Converters;

/// <summary>Converts an int to Visibility. If ConverterParameter is set, returns Visible when value equals the parameter.</summary>
public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal && parameter is string paramStr && int.TryParse(paramStr, out int target))
        {
            return intVal == target ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts a count (int) > 0 to Visibility.Visible, else Collapsed.</summary>
public class IntToBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts a decimal to a bar width (clamped to a max). Used for simple inline bar charts.</summary>
public class DecimalToBarWidthConverter : IValueConverter
{
    public double MaxWidth { get; set; } = 300;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            // Simple proportional: Scale based on max amount seen (we'll use log scale for large variance)
            if (amount <= 0) return 0.0;
            // Clamp to reasonable bar width using a simple ratio approach
            var width = Math.Min((double)amount / 100.0 * 3.0, MaxWidth);
            return Math.Max(width, 4.0); // Minimum 4px so it's visible
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts a non-empty string to Visibility.Visible, empty/null to Collapsed.</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Shows Visible when int == 0 (for "no data" messages). Collapsed otherwise.</summary>
public class IntToZeroVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
