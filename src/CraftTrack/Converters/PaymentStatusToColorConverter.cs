using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CraftTrack.Models.Enums;

namespace CraftTrack.Converters;

public class PaymentStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PaymentType paymentType)
        {
            var key = paymentType switch
            {
                PaymentType.FullPayment => "PaidColor",
                PaymentType.Downpayment => "PartialColor",
                PaymentType.Unpaid => "UnpaidColor",
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
