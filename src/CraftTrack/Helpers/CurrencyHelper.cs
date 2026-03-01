using System.Globalization;

namespace CraftTrack.Helpers;

public static class CurrencyHelper
{
    private const string CurrencySymbol = "₱";

    public static string Format(decimal amount)
    {
        return $"{CurrencySymbol}{amount:N2}";
    }

    public static string FormatShort(decimal amount)
    {
        if (amount >= 1_000_000)
            return $"{CurrencySymbol}{amount / 1_000_000:N1}M";
        if (amount >= 1_000)
            return $"{CurrencySymbol}{amount / 1_000:N1}K";
        return Format(amount);
    }
}
