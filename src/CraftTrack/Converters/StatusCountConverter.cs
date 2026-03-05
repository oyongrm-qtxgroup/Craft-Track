using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using CraftTrack.ViewModels;

namespace CraftTrack.Converters;

/// <summary>
/// Extracts the count for a specific OrderStatus from the StatusBreakdown collection.
/// ConverterParameter should be "Active", "InProgress", "Completed", or "Cancelled".
/// </summary>
public class StatusCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<StatusBreakdownItem> items && parameter is string statusName)
        {
            var item = items.FirstOrDefault(s => s.StatusName.Replace(" ", "") == statusName);
            return item?.Count.ToString("N0") ?? "0";
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
