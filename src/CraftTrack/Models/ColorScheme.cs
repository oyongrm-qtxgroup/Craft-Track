namespace CraftTrack.Models;

/// <summary>
/// Represents a configurable color scheme with 6 base colors.
/// All colors are stored as hex strings (e.g. "#7C3AED").
/// The app derives all ~90 theme resources from these base colors.
/// </summary>
public class ColorScheme
{
    /// <summary>Display name of the scheme (e.g. "Purple", "Ocean Blue").</summary>
    public string Name { get; set; } = "Purple";

    /// <summary>Primary accent color used for highlights, selected states, sidebar, tabs, etc.</summary>
    public string AccentColor { get; set; } = "#7C3AED";

    /// <summary>Secondary accent color used for stat icons, notification headers, pills, etc.</summary>
    public string SecondaryColor { get; set; } = "#EC4899";

    /// <summary>Success / positive status color (paid, low severity, profit, revenue).</summary>
    public string SuccessColor { get; set; } = "#10B981";

    /// <summary>Warning / caution color (partial payment, high severity, outstanding).</summary>
    public string WarningColor { get; set; } = "#F59E0B";

    /// <summary>Danger / error color (unpaid, rush severity, expenses, rejects).</summary>
    public string DangerColor { get; set; } = "#EF4444";

    /// <summary>Sidebar gradient base color (top of gradient). Bottom is derived from SecondaryColor.</summary>
    public string SidebarColor { get; set; } = "#5B21B6";

    /// <summary>Creates a deep copy of this color scheme.</summary>
    public ColorScheme Clone() => new()
    {
        Name = Name,
        AccentColor = AccentColor,
        SecondaryColor = SecondaryColor,
        SuccessColor = SuccessColor,
        WarningColor = WarningColor,
        DangerColor = DangerColor,
        SidebarColor = SidebarColor
    };

    /// <summary>Returns true if all color values match another scheme.</summary>
    public bool ColorsEqual(ColorScheme other)
    {
        if (other is null) return false;
        return string.Equals(AccentColor, other.AccentColor, StringComparison.OrdinalIgnoreCase)
            && string.Equals(SecondaryColor, other.SecondaryColor, StringComparison.OrdinalIgnoreCase)
            && string.Equals(SuccessColor, other.SuccessColor, StringComparison.OrdinalIgnoreCase)
            && string.Equals(WarningColor, other.WarningColor, StringComparison.OrdinalIgnoreCase)
            && string.Equals(DangerColor, other.DangerColor, StringComparison.OrdinalIgnoreCase)
            && string.Equals(SidebarColor, other.SidebarColor, StringComparison.OrdinalIgnoreCase);
    }
}
