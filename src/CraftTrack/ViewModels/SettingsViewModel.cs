using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Services;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private bool _suppressColorSync;

    [ObservableProperty]
    private ThemeMode _selectedTheme;

    // ── Color Scheme ──

    [ObservableProperty]
    private string _selectedPresetName = "Purple";

    [ObservableProperty]
    private bool _isCustom;

    [ObservableProperty]
    private Color _accentColor;

    [ObservableProperty]
    private Color _secondaryColor;

    [ObservableProperty]
    private Color _successColor;

    [ObservableProperty]
    private Color _warningColor;

    [ObservableProperty]
    private Color _dangerColor;

    [ObservableProperty]
    private Color _sidebarColor;

    public ObservableCollection<ColorScheme> Presets { get; } = new(ColorPresets.All);

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        _selectedTheme = _themeService.CurrentMode;

        // Initialize color properties from current scheme
        _suppressColorSync = true;
        LoadSchemeIntoProperties(_themeService.CurrentColorScheme);
        _selectedPresetName = _themeService.CurrentColorScheme.Name;
        _isCustom = false;
        _suppressColorSync = false;
    }

    partial void OnSelectedThemeChanged(ThemeMode value)
    {
        _themeService.SetTheme(value);
    }

    partial void OnSelectedPresetNameChanged(string value)
    {
        if (_suppressColorSync) return;

        var preset = ColorPresets.GetByName(value);
        _suppressColorSync = true;
        LoadSchemeIntoProperties(preset);
        IsCustom = false;
        _suppressColorSync = false;

        _themeService.SetPreset(value);
    }

    partial void OnAccentColorChanged(Color value) => PushColorChange();
    partial void OnSecondaryColorChanged(Color value) => PushColorChange();
    partial void OnSuccessColorChanged(Color value) => PushColorChange();
    partial void OnWarningColorChanged(Color value) => PushColorChange();
    partial void OnDangerColorChanged(Color value) => PushColorChange();
    partial void OnSidebarColorChanged(Color value) => PushColorChange();

    [RelayCommand]
    private void ResetToPreset()
    {
        var preset = ColorPresets.GetByName(SelectedPresetName);
        _suppressColorSync = true;
        LoadSchemeIntoProperties(preset);
        IsCustom = false;
        _suppressColorSync = false;

        _themeService.SetPreset(SelectedPresetName);
    }

    private void PushColorChange()
    {
        if (_suppressColorSync) return;

        var scheme = BuildSchemeFromProperties();

        // Check if it matches the current preset
        var preset = ColorPresets.GetByName(SelectedPresetName);
        IsCustom = !scheme.ColorsEqual(preset);

        _themeService.SetColorScheme(scheme);
    }

    private ColorScheme BuildSchemeFromProperties() => new()
    {
        Name = SelectedPresetName,
        AccentColor = ColorToHex(AccentColor),
        SecondaryColor = ColorToHex(SecondaryColor),
        SuccessColor = ColorToHex(SuccessColor),
        WarningColor = ColorToHex(WarningColor),
        DangerColor = ColorToHex(DangerColor),
        SidebarColor = ColorToHex(SidebarColor)
    };

    private void LoadSchemeIntoProperties(ColorScheme scheme)
    {
        AccentColor = ParseHex(scheme.AccentColor);
        SecondaryColor = ParseHex(scheme.SecondaryColor);
        SuccessColor = ParseHex(scheme.SuccessColor);
        WarningColor = ParseHex(scheme.WarningColor);
        DangerColor = ParseHex(scheme.DangerColor);
        SidebarColor = ParseHex(scheme.SidebarColor);
    }

    private static Color ParseHex(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.Gray; }
    }

    private static string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}
