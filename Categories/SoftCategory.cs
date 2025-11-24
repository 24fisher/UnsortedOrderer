using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class SoftCategory : FileCategory, INonSplittableDirectoryCategory
{
    private const string DiskImagesFolderName = "_Образы дисков";

    private static readonly SoftwareDistributivesDetector DistributionDetector = new();
    private static readonly string[] DiskImageExtensions =
    {
        ".iso",
        ".cso",
        ".dmg",
        ".img",
        ".mdf",
        ".mds",
        ".nrg",
        ".ccd",
        ".isz",
        ".vhd",
        ".vhdx"
    };

    private static readonly string[] SoftExtensions =
        new[]
        {
            ".exe",
            ".msi",
            ".msix",
            ".apk",
            ".pkg",
            ".deb",
            ".rpm",
            ".appimage"
        }
        .Concat(DiskImageExtensions)
        .ToArray();

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
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (DiskImageExtensions.Contains(extension))
        {
            var diskImagesFolder = Path.Combine(categoryRoot, DiskImagesFolderName);
            Directory.CreateDirectory(diskImagesFolder);

            return diskImagesFolder;
        }

        return DistributionFolderHelper.GetDistributionDestinationDirectory(categoryRoot, filePath);
    }
}
