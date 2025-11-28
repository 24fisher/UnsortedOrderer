using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class SoftCategory : FileCategory, INonSplittableDirectoryCategory
{
    private const string DiskImagesFolderName = "_Образы дисков";
    private const string ConfigurationFilesFolderName = "_Configuration_files";
    private const string XmlFilesFolderName = "_Xml_files";

    private readonly SoftwareDistributivesDetector _distributionDetector;
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
        .Append(".conf")
        .Append(".xml")
        .Concat(DiskImageExtensions)
        .ToArray();

    public SoftCategory(string folderName, SoftwareDistributivesDetector distributionDetector)
        : base("Soft", folderName, SoftExtensions)
    {
        _distributionDetector = distributionDetector;
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return _distributionDetector.IsFolderOfCategory<SoftCategory>(path);
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
        if (extension == ".conf")
        {
            var configurationFolder = Path.Combine(categoryRoot, ConfigurationFilesFolderName);
            Directory.CreateDirectory(configurationFolder);

            return configurationFolder;
        }

        if (extension == ".xml")
        {
            var xmlFolder = Path.Combine(categoryRoot, XmlFilesFolderName);
            Directory.CreateDirectory(xmlFolder);

            return xmlFolder;
        }

        if (DiskImageExtensions.Contains(extension))
        {
            var diskImagesFolder = Path.Combine(categoryRoot, DiskImagesFolderName);
            Directory.CreateDirectory(diskImagesFolder);

            return diskImagesFolder;
        }

        return DistributionFolderHelper.GetDistributionDestinationDirectory(categoryRoot, filePath);
    }
}
