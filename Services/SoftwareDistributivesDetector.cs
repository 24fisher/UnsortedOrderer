using System.Text.RegularExpressions;

namespace UnsortedOrderer.Services;

public interface IDistributionDetector
{
    bool IsDistributionDirectory(string path);
}

public sealed class SoftwareDistributivesDetector : IDistributionDetector
{
    private static readonly string[] InstallerExtensions = [
        ".exe", ".msi", ".msix", ".apk", ".dmg", ".pkg", ".deb", ".rpm", ".appimage", ".iso"
    ];

    private static readonly Regex SetupKeywords = new("(setup|install|installer|driver)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool IsDistributionDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        var folderName = Path.GetFileName(path) ?? string.Empty;
        if (IsSoftFolder(folderName))
        {
            return false;
        }

        var topLevelFiles = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
        if (topLevelFiles.Any(file => MatchesInstaller(file)))
        {
            return true;
        }

        return SetupKeywords.IsMatch(folderName);
    }

    private static bool MatchesInstaller(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (InstallerExtensions.Contains(extension))
        {
            return true;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        return SetupKeywords.IsMatch(fileName);
    }

    private static bool IsSoftFolder(string folderName)
    {
        var normalizedName = folderName.TrimStart('_');

        return normalizedName.Equals("soft", StringComparison.OrdinalIgnoreCase)
            || normalizedName.Equals("софт", StringComparison.OrdinalIgnoreCase);
    }
}
