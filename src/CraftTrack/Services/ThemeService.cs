using System.IO;
using System.Text.Json;
using System.Windows;
using CraftTrack.Helpers;
using CraftTrack.Models;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.Services;

public class ThemeService : IThemeService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CraftTrack");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly Uri LightColorsUri = new("Themes/Colors.Light.xaml", UriKind.Relative);
    private static readonly Uri DarkColorsUri = new("Themes/Colors.Dark.xaml", UriKind.Relative);

    public ThemeMode CurrentMode { get; private set; } = ThemeMode.System;
    public bool IsDarkTheme { get; private set; }
    public ColorScheme CurrentColorScheme { get; private set; } = ColorPresets.Purple.Clone();

    public event Action? ThemeChanged;
    public event Action? ColorSchemeChanged;

    private AppSettings _settings = new();

    public void Initialize()
    {
        _settings = LoadSettings();
        CurrentMode = _settings.Theme;

        // Restore color scheme from settings
        if (_settings.CustomColorScheme is not null)
        {
            CurrentColorScheme = _settings.CustomColorScheme.Clone();
        }
        else
        {
            CurrentColorScheme = ColorPresets.GetByName(_settings.ColorPresetName).Clone();
        }

        // Subscribe to system theme changes
        SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;

        ApplyTheme(CurrentMode);
    }

    public void SetTheme(ThemeMode mode)
    {
        CurrentMode = mode;
        _settings.Theme = mode;
        ApplyTheme(mode);
        SaveSettings(_settings);
        ThemeChanged?.Invoke();
    }

    public void SetPreset(string presetName)
    {
        var preset = ColorPresets.GetByName(presetName);
        CurrentColorScheme = preset.Clone();
        _settings.ColorPresetName = preset.Name;
        _settings.CustomColorScheme = null; // clear custom overrides
        ApplyColorOverrides();
        SaveSettings(_settings);
        ColorSchemeChanged?.Invoke();
    }

    public void SetColorScheme(ColorScheme scheme)
    {
        CurrentColorScheme = scheme.Clone();
        _settings.CustomColorScheme = scheme.Clone();
        _settings.ColorPresetName = scheme.Name;
        ApplyColorOverrides();
        SaveSettings(_settings);
        ColorSchemeChanged?.Invoke();
    }

    private void ApplyTheme(ThemeMode mode)
    {
        bool dark = mode switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            ThemeMode.System => IsSystemDark(),
            _ => false
        };

        IsDarkTheme = dark;

        // 1) Swap our custom color dictionary
        SwapColorDictionary(dark);

        // 2) Update MaterialDesign base theme + primary/secondary from current scheme
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(dark ? BaseTheme.Dark : BaseTheme.Light);

        // Use accent and secondary from current color scheme
        var primary = ParseColorSafe(CurrentColorScheme.AccentColor, 0x7C, 0x3A, 0xED);
        var secondary = ParseColorSafe(CurrentColorScheme.SecondaryColor, 0xEC, 0x48, 0x99);
        theme.SetPrimaryColor(primary);
        theme.SetSecondaryColor(secondary);

        paletteHelper.SetTheme(theme);

        // 3) Apply color scheme overrides on top of the base dictionary
        ApplyColorOverrides();
    }

    private void ApplyColorOverrides()
    {
        ColorDerivationHelper.ApplyColorScheme(CurrentColorScheme, IsDarkTheme);
    }

    private static System.Windows.Media.Color ParseColorSafe(string hex, byte r, byte g, byte b)
    {
        try
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }
    }

    private void SwapColorDictionary(bool dark)
    {
        var app = Application.Current;
        if (app == null) return;

        var targetUri = dark ? DarkColorsUri : LightColorsUri;

        // Find and remove the existing color dictionary
        ResourceDictionary? existing = null;
        foreach (var dict in app.Resources.MergedDictionaries)
        {
            if (dict.Source != null &&
                (dict.Source.OriginalString.Contains("Colors.Light", StringComparison.OrdinalIgnoreCase) ||
                 dict.Source.OriginalString.Contains("Colors.Dark", StringComparison.OrdinalIgnoreCase)))
            {
                existing = dict;
                break;
            }
        }

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        // Insert the new color dictionary at the beginning so styles in ModernTheme.xaml can find them
        app.Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = targetUri });
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intVal)
                return intVal == 0; // 0 = dark
        }
        catch
        {
            // Fallback to light if registry read fails
        }

        return false;
    }

    private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General && CurrentMode == ThemeMode.System)
        {
            // Re-apply when system mode changes
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                ApplyTheme(ThemeMode.System);
                ThemeChanged?.Invoke();
            });
        }
    }

    private static AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // If settings file is corrupted, return defaults
        }

        return new AppSettings();
    }

    private static void SaveSettings(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail if settings can't be saved
        }
    }
}
