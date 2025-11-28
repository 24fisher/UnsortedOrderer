using System.IO;
using UnsortedOrderer.Application.Contracts.Services.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class MusicCategory : FileCategory, INonSplittableDirectoryCategory
{
    internal static readonly string[] MusicExtensions =
    [
        ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".wma", ".aiff", ".alac", ".opus"
    ];

    private readonly ICategoryParsingService _musicCategoryParsingService;

    public MusicCategory(string folderName, IMusicDirectoryDetector musicDirectoryDetector)
        : base("Music", folderName, MusicExtensions)
    {
        _musicCategoryParsingService = musicDirectoryDetector as ICategoryParsingService
            ?? throw new ArgumentNullException(nameof(musicDirectoryDetector), "IMusicDirectoryDetector должен реализовывать IFileCategoryParsingService.");
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return _musicCategoryParsingService.IsFolderOfCategory<MusicCategory>(path);
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        var categoryRoot = Path.Combine(destinationRoot, FolderName);
        Directory.CreateDirectory(categoryRoot);

        var directoryName = Path.GetFileName(directoryPath) ?? "Music";
        return FileUtilities.GetUniqueDirectoryPath(categoryRoot, directoryName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }
}
