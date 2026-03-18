using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Services;
using Microsoft.Win32;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.ViewModels;

public partial class SetupWizardViewModel : ObservableObject
{
    private readonly IBrandingService _brandingService;
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private string _appName = "Craft Track";

    [ObservableProperty]
    private string _appSubtitle = "Your small business helper at hand";

    [ObservableProperty]
    private string? _logoPngPath;

    [ObservableProperty]
    private ImageSource? _logoPreview;

    [ObservableProperty]
    private ThemeMode _selectedTheme;

    [ObservableProperty]
    private string _selectedPresetName = "Purple";

    public ObservableCollection<ColorScheme> Presets { get; } = new(ColorPresets.All);

    public bool CanGoBack => CurrentStep > 0;
    public bool IsFinalStep => CurrentStep >= 3;
    public string NextButtonText => CurrentStep == 0 ? "Get Started" : CurrentStep == 2 ? "Review" : "Next";

    public event Action<bool>? RequestClose;

    public SetupWizardViewModel(IBrandingService brandingService, IThemeService themeService)
    {
        _brandingService = brandingService;
        _themeService = themeService;

        AppName = brandingService.AppName;
        AppSubtitle = brandingService.AppSubtitle;
        LogoPreview = brandingService.LogoImageSource;
        SelectedTheme = themeService.CurrentMode;
        SelectedPresetName = themeService.CurrentColorScheme.Name;
    }

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsFinalStep));
        OnPropertyChanged(nameof(NextButtonText));
    }

    [RelayCommand]
    private void Next()
    {
        if (CurrentStep < 3)
            CurrentStep++;
    }

    [RelayCommand]
    private void Back()
    {
        if (CurrentStep > 0)
            CurrentStep--;
    }

    [RelayCommand]
    private void SetTheme(ThemeMode theme)
    {
        SelectedTheme = theme;
    }

    [RelayCommand]
    private void SelectPreset(string? presetName)
    {
        if (!string.IsNullOrWhiteSpace(presetName))
            SelectedPresetName = presetName;
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PNG files (*.png)|*.png",
            Multiselect = false,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            LogoPngPath = dialog.FileName;
            LogoPreview = new ImageSourceConverter().ConvertFromString(dialog.FileName) as ImageSource;
        }
    }

    [RelayCommand]
    private void Finish()
    {
        _themeService.SetTheme(SelectedTheme);
        _themeService.SetPreset(SelectedPresetName);
        _brandingService.SetBranding(AppName, AppSubtitle, LogoPngPath);

        RequestClose?.Invoke(true);
    }
}
