using System.Collections.ObjectModel;

namespace CraftTrack.Models;

/// <summary>
/// Built-in color presets that ship with CraftTrack.
/// </summary>
public static class ColorPresets
{
    public static ColorScheme Purple { get; } = new()
    {
        Name = "Purple",
        AccentColor = "#7C3AED",
        SecondaryColor = "#EC4899",
        SuccessColor = "#10B981",
        WarningColor = "#F59E0B",
        DangerColor = "#EF4444",
        SidebarColor = "#5B21B6"
    };

    public static ColorScheme OceanBlue { get; } = new()
    {
        Name = "Ocean Blue",
        AccentColor = "#2563EB",
        SecondaryColor = "#06B6D4",
        SuccessColor = "#10B981",
        WarningColor = "#F59E0B",
        DangerColor = "#EF4444",
        SidebarColor = "#1E40AF"
    };

    public static ColorScheme ForestGreen { get; } = new()
    {
        Name = "Forest Green",
        AccentColor = "#059669",
        SecondaryColor = "#34D399",
        SuccessColor = "#22C55E",
        WarningColor = "#EAB308",
        DangerColor = "#EF4444",
        SidebarColor = "#065F46"
    };

    public static ColorScheme RosePink { get; } = new()
    {
        Name = "Rose Pink",
        AccentColor = "#E11D48",
        SecondaryColor = "#F472B6",
        SuccessColor = "#10B981",
        WarningColor = "#F59E0B",
        DangerColor = "#DC2626",
        SidebarColor = "#9F1239"
    };

    public static ColorScheme AmberGold { get; } = new()
    {
        Name = "Amber Gold",
        AccentColor = "#D97706",
        SecondaryColor = "#FBBF24",
        SuccessColor = "#10B981",
        WarningColor = "#EA580C",
        DangerColor = "#EF4444",
        SidebarColor = "#92400E"
    };

    public static ColorScheme Slate { get; } = new()
    {
        Name = "Slate",
        AccentColor = "#475569",
        SecondaryColor = "#94A3B8",
        SuccessColor = "#10B981",
        WarningColor = "#F59E0B",
        DangerColor = "#EF4444",
        SidebarColor = "#1E293B"
    };

    /// <summary>All built-in presets.</summary>
    public static ReadOnlyCollection<ColorScheme> All { get; } = new List<ColorScheme>
    {
        Purple,
        OceanBlue,
        ForestGreen,
        RosePink,
        AmberGold,
        Slate
    }.AsReadOnly();

    /// <summary>Returns the preset matching the given name, or <see cref="Purple"/> if not found.</summary>
    public static ColorScheme GetByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Purple;

        foreach (var preset in All)
        {
            if (preset.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return preset;
        }

        return Purple;
    }
}
