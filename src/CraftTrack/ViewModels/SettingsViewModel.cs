using CommunityToolkit.Mvvm.ComponentModel;
using CraftTrack.Models;
using CraftTrack.Services;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private ThemeMode _selectedTheme;

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        _selectedTheme = _themeService.CurrentMode;
    }

    partial void OnSelectedThemeChanged(ThemeMode value)
    {
        _themeService.SetTheme(value);
    }
}
