using System.IO;
using System.Runtime.InteropServices;

namespace CraftTrack.Helpers;

public static class ShortcutHelper
{
    public static void UpdateAllShortcuts(string icoPath)
    {
        if (string.IsNullOrWhiteSpace(icoPath) || !File.Exists(icoPath))
            return;

        foreach (var shortcutPath in GetShortcutCandidates().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(shortcutPath))
                continue;

            try
            {
                UpdateShortcutIcon(shortcutPath, icoPath);
            }
            catch
            {
                // Shortcuts might not exist or be writable depending on install options and permissions.
            }
        }
    }

    private static IEnumerable<string> GetShortcutCandidates()
    {
        var userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var publicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        var userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        var commonPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

        yield return Path.Combine(userDesktop, "CraftTrack.lnk");
        yield return Path.Combine(publicDesktop, "CraftTrack.lnk");
        yield return Path.Combine(userPrograms, "CraftTrack", "CraftTrack.lnk");
        yield return Path.Combine(commonPrograms, "CraftTrack", "CraftTrack.lnk");
        yield return Path.Combine(userPrograms, "CraftTrack.lnk");
        yield return Path.Combine(commonPrograms, "CraftTrack.lnk");
    }

    private static void UpdateShortcutIcon(string shortcutPath, string icoPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null)
            return;

        object? shellObject = null;
        object? shortcutObject = null;

        try
        {
            shellObject = Activator.CreateInstance(shellType);
            if (shellObject is null)
                return;

            dynamic shell = shellObject;
            shortcutObject = shell.CreateShortcut(shortcutPath);
            dynamic shortcut = shortcutObject;
            shortcut.IconLocation = $"{icoPath},0";
            shortcut.Save();
        }
        finally
        {
            if (shortcutObject is not null && Marshal.IsComObject(shortcutObject))
                Marshal.FinalReleaseComObject(shortcutObject);
            if (shellObject is not null && Marshal.IsComObject(shellObject))
                Marshal.FinalReleaseComObject(shellObject);
        }
    }
}
