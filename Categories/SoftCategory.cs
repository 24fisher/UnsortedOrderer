using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class SoftCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly SoftwareDistributivesDetector DistributionDetector = new();
    private static readonly string[] SoftExtensions =
    [
        ".exe", ".msi", ".msix", ".apk", ".dmg", ".pkg", ".deb", ".rpm", ".appimage", ".iso"
    ];

    public SoftCategory(string folderName)
        : base("Soft", folderName, SoftExtensions)
    {
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return DistributionDetector.IsDistributionDirectory(path);
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        Directory.CreateDirectory(categoryRoot);

        var directoryName = Path.GetFileName(directoryPath) ?? "Distribution";
        return FileUtilities.GetUniqueDirectoryPath(categoryRoot, directoryName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        return DistributionFolderHelper.GetDistributionDestinationDirectory(categoryRoot, filePath);
    }
}
