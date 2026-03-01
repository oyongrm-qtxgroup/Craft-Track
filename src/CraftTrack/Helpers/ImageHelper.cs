using System.IO;

namespace CraftTrack.Helpers;

public static class ImageHelper
{
    private static readonly string ImagesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "CraftTrack", "Images");

    public static string GetImagesDirectory()
    {
        Directory.CreateDirectory(ImagesFolder);
        return ImagesFolder;
    }

    public static string SaveProductImage(string sourceFilePath)
    {
        Directory.CreateDirectory(ImagesFolder);

        var extension = Path.GetExtension(sourceFilePath);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var destPath = Path.Combine(ImagesFolder, fileName);

        File.Copy(sourceFilePath, destPath, overwrite: true);
        return fileName; // relative name only
    }

    public static void DeleteProductImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var fullPath = GetFullImagePath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public static string GetFullImagePath(string relativePath)
    {
        return Path.Combine(ImagesFolder, relativePath);
    }

    public static bool ImageExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;
        return File.Exists(GetFullImagePath(relativePath));
    }
}
