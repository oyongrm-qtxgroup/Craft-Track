using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CraftTrack.Helpers;

namespace CraftTrack.Converters;

/// <summary>
/// Converts a relative image path (filename) to a BitmapImage for display.
/// Returns null if path is empty or file doesn't exist (allowing placeholder to show through).
/// </summary>
public class ImagePathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string relativePath || string.IsNullOrWhiteSpace(relativePath))
            return null;

        var fullPath = ImageHelper.GetFullImagePath(relativePath);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 400;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
