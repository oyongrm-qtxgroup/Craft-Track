using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CraftTrack.Services;

public interface IBrandingService
{
    string AppName { get; }
    string AppSubtitle { get; }
    ImageSource? LogoImageSource { get; }
    BitmapFrame? IcoImageSource { get; }
    bool IsSetupComplete { get; }

    event Action? BrandingChanged;

    void Initialize();
    void SetBranding(string name, string subtitle, string? logoPngSourcePath = null, string? icoSourcePath = null);
    string? CopyLogoToAppFolder(string sourcePath);
    string? CopyIcoToAppFolder(string sourcePath);
}
