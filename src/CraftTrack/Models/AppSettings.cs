namespace CraftTrack.Models;

/// <summary>
/// Theme mode options for the application.
/// </summary>
public enum ThemeMode
{
    Light,
    Dark,
    System
}

/// <summary>
/// Persisted application settings.
/// </summary>
public class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.System;

    /// <summary>Name of the selected color preset (e.g. "Purple", "Ocean Blue"). Null means default.</summary>
    public string? ColorPresetName { get; set; }

    /// <summary>Custom color overrides. Null when using a built-in preset without modifications.</summary>
    public ColorScheme? CustomColorScheme { get; set; }
}
