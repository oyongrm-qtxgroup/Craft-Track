using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CraftTrack.Helpers;
using CraftTrack.Models;

namespace CraftTrack.Services;

public class BrandingService : IBrandingService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CraftTrack");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "branding.json");
    private static readonly Uri DefaultLogoUri = new("pack://application:,,,/Assets/Craft%20Track%20(Icon).png", UriKind.Absolute);
    private static readonly Uri DefaultIcoUri = new("pack://application:,,,/Craft%20Track%20(Icon).ico", UriKind.Absolute);

    private BrandingSettings _settings = new();

    public string AppName { get; private set; } = "Craft Track";
    public string AppSubtitle { get; private set; } = "Your small business helper at hand";
    public ImageSource? LogoImageSource { get; private set; }
    public BitmapFrame? IcoImageSource { get; private set; }
    public bool IsSetupComplete => _settings.IsSetupComplete;

    public event Action? BrandingChanged;

    public void Initialize()
    {
        _settings = LoadSettings();
        AppName = string.IsNullOrWhiteSpace(_settings.AppName) ? "Craft Track" : _settings.AppName.Trim();
        AppSubtitle = string.IsNullOrWhiteSpace(_settings.AppSubtitle)
            ? "Your small business helper at hand"
            : _settings.AppSubtitle.Trim();

        LogoImageSource = LoadLogoImage(_settings.LogoPath) ?? LoadDefaultLogoImage();
        IcoImageSource = LoadIcoImage(_settings.IcoPath) ?? LoadDefaultIconFrame();
    }

    public void SetBranding(string name, string subtitle, string? logoPngSourcePath = null, string? icoSourcePath = null)
    {
        _settings.AppName = string.IsNullOrWhiteSpace(name) ? "Craft Track" : name.Trim();
        _settings.AppSubtitle = string.IsNullOrWhiteSpace(subtitle)
            ? "Your small business helper at hand"
            : subtitle.Trim();

        if (!string.IsNullOrWhiteSpace(logoPngSourcePath))
        {
            var copiedLogoPath = CopyLogoToAppFolder(logoPngSourcePath);
            if (!string.IsNullOrWhiteSpace(copiedLogoPath))
                _settings.LogoPath = copiedLogoPath;
        }

        if (!string.IsNullOrWhiteSpace(icoSourcePath))
        {
            var copiedIcoPath = CopyIcoToAppFolder(icoSourcePath);
            if (!string.IsNullOrWhiteSpace(copiedIcoPath))
                _settings.IcoPath = copiedIcoPath;
        }

        _settings.IsSetupComplete = true;
        SaveSettings(_settings);

        AppName = _settings.AppName;
        AppSubtitle = _settings.AppSubtitle;
        LogoImageSource = LoadLogoImage(_settings.LogoPath) ?? LoadDefaultLogoImage();
        IcoImageSource = LoadIcoImage(_settings.IcoPath) ?? LoadDefaultIconFrame();

        if (!string.IsNullOrWhiteSpace(_settings.IcoPath))
            ShortcutHelper.UpdateAllShortcuts(_settings.IcoPath);

        BrandingChanged?.Invoke();
    }

    public string? CopyLogoToAppFolder(string sourcePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            Directory.CreateDirectory(SettingsDir);
            var destinationPath = Path.Combine(SettingsDir, "logo_user.png");
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return destinationPath;
        }
        catch
        {
            return null;
        }
    }

    public string? CopyIcoToAppFolder(string sourcePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            Directory.CreateDirectory(SettingsDir);
            var destinationPath = Path.Combine(SettingsDir, "app_user.ico");
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return destinationPath;
        }
        catch
        {
            return null;
        }
    }

    private static BrandingSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<BrandingSettings>(json) ?? new BrandingSettings();
            }
        }
        catch
        {
            // Ignore and use defaults if settings file is unreadable.
        }

        return new BrandingSettings();
    }

    private static void SaveSettings(BrandingSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ignore save errors so branding updates do not crash UI.
        }
    }

    private static BitmapImage? LoadLogoImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private static BitmapImage? LoadDefaultLogoImage()
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = DefaultLogoUri;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private static BitmapFrame? LoadIcoImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            var frame = BitmapFrame.Create(new Uri(path, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            frame.Freeze();
            return frame;
        }
        catch
        {
            return null;
        }
    }

    private static BitmapFrame? LoadDefaultIconFrame()
    {
        try
        {
            var frame = BitmapFrame.Create(DefaultIcoUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            frame.Freeze();
            return frame;
        }
        catch
        {
            return null;
        }
    }
}
