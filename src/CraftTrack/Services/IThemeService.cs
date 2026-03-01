using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.Services;

public interface IThemeService
{
    /// <summary>Current user-selected theme mode (Light / Dark / System).</summary>
    ThemeMode CurrentMode { get; }

    /// <summary>Whether the effectively-applied theme is dark.</summary>
    bool IsDarkTheme { get; }

    /// <summary>Apply a theme mode. Swaps color dictionaries, updates MaterialDesign, and persists the choice.</summary>
    void SetTheme(ThemeMode mode);

    /// <summary>Load persisted settings and apply the saved theme. Call once at startup.</summary>
    void Initialize();

    /// <summary>Raised after the theme is changed.</summary>
    event Action? ThemeChanged;
}
