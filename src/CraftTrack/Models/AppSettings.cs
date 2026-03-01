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
}
