using System.IO;
using System.Text.Json;
using System.Windows;
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

    public event Action? ThemeChanged;

    public void Initialize()
    {
        var settings = LoadSettings();
        CurrentMode = settings.Theme;

        // Subscribe to system theme changes
        SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;

        ApplyTheme(CurrentMode);
    }

    public void SetTheme(ThemeMode mode)
    {
        CurrentMode = mode;
        ApplyTheme(mode);
        SaveSettings(new AppSettings { Theme = mode });
        ThemeChanged?.Invoke();
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

        // 2) Update MaterialDesign base theme + primary/secondary colors
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(dark ? BaseTheme.Dark : BaseTheme.Light);

        // Purple primary #7C3AED  /  Pink secondary #EC4899
        var primary = System.Windows.Media.Color.FromRgb(0x7C, 0x3A, 0xED);
        var secondary = System.Windows.Media.Color.FromRgb(0xEC, 0x48, 0x99);
        theme.SetPrimaryColor(primary);
        theme.SetSecondaryColor(secondary);

        paletteHelper.SetTheme(theme);
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
