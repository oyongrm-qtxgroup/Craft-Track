namespace CraftTrack.Models;

public class BrandingSettings
{
    public string AppName { get; set; } = "Craft Track";
    public string AppSubtitle { get; set; } = "Your small business helper at hand";
    public string? LogoPath { get; set; }
    public string? IcoPath { get; set; }
    public bool IsSetupComplete { get; set; }
}
