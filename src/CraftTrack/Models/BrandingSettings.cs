namespace CraftTrack.Models;

public class BrandingSettings
{
    public string AppName { get; set; } = "CraftTrack";
    public string AppSubtitle { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? IcoPath { get; set; }
    public bool IsSetupComplete { get; set; }
}
