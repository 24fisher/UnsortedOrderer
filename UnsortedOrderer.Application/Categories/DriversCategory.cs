using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class DriversCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] InstallerExtensions =
    {
        ".exe",
        ".msi",
        ".msix",
        ".msixbundle",
        ".msu"
    };

    private static readonly string[] DriverNameMarkers =
    {
        "driver",
        "драйвер"
    };

    private static readonly string[] DriverExtensions =
    [
        ".inf",
        ".sys",
        ".cat",
        ".drv",
        ".cab"
    ];

    public DriversCategory(string folderName)
        : base("Drivers", folderName, DriverExtensions)
    {
    }

    public bool IsNonSplittableDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        var directoryName = Path.GetFileName(path);
        if (HasDriverMarker(directoryName))
        {
            return true;
        }

        return Directory
            .EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Any(IsDriverFile);
    }

    public bool IsDriverFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        if (Matches(extension))
        {
            return true;
        }

        if (!InstallerExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return HasDriverMarker(fileName)
            || HasDriverMarker(Path.GetFileName(Path.GetDirectoryName(filePath)));
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        Directory.CreateDirectory(categoryRoot);

        var directoryName = Path.GetFileName(directoryPath) ?? "Drivers";
        return FileUtilities.GetUniqueDirectoryPath(categoryRoot, directoryName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }

    private static bool HasDriverMarker(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return DriverNameMarkers.Any(marker => name.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
