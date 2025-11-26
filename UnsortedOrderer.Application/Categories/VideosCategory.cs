using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class VideosCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".mpeg"
    ];

    private readonly IReadOnlyCollection<DeviceBrandPattern> _cameraPatterns;

    public VideosCategory(IReadOnlyCollection<DeviceBrandPattern> cameraPatterns)
        : base("Videos", "Videos", VideoExtensions)
    {
        _cameraPatterns = cameraPatterns ?? Array.Empty<DeviceBrandPattern>();
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return false;
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        var year = GetVideoYear(filePath);
        var cameraFolder = GetCameraSubfolder(Path.GetFileName(filePath));
        var baseDirectory = Path.Combine(destinationRoot, FolderName, year.ToString());

        return cameraFolder is null
            ? baseDirectory
            : Path.Combine(baseDirectory, cameraFolder);
    }

    private string? GetCameraSubfolder(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return DeviceBrandMatcher.GetBrandByFileName(fileName, _cameraPatterns);
    }

    private static int GetVideoYear(string filePath)
    {
        try
        {
            var creationTime = File.GetCreationTime(filePath);
            if (creationTime.Year > 1)
            {
                return creationTime.Year;
            }

            var lastWriteTime = File.GetLastWriteTime(filePath);
            if (lastWriteTime.Year > 1)
            {
                return lastWriteTime.Year;
            }
        }
        catch
        {
            return DateTime.Now.Year;
        }

        return DateTime.Now.Year;
    }
}
