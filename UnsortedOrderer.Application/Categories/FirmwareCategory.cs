using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class FirmwareCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] FirmwareExtensions =
    [
        ".bin",
        ".dat"
    ];

    public FirmwareCategory(string folderName)
        : base("Firmware", folderName, FirmwareExtensions)
    {
    }

    public bool IsNonSplittableDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        return Directory
            .EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Any(file => Matches(Path.GetExtension(file)));
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        Directory.CreateDirectory(categoryRoot);

        var directoryName = Path.GetFileName(directoryPath) ?? "Firmware";
        return FileUtilities.GetUniqueDirectoryPath(categoryRoot, directoryName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }
}
