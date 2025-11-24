using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class FileOrganizerService
{
    private readonly AppSettings _settings;
    private readonly IDistributionDetector _distributionDetector;
    private readonly IArchiveService _archiveService;
    private readonly IPhotoService _photoService;
    private readonly IReadOnlyCollection<IFileCategory> _categories;

    public FileOrganizerService(
        AppSettings settings,
        IDistributionDetector distributionDetector,
        IArchiveService archiveService,
        IPhotoService photoService,
        IEnumerable<IFileCategory> categories)
    {
        _settings = settings;
        _distributionDetector = distributionDetector;
        _archiveService = archiveService;
        _photoService = photoService;
        _categories = categories.ToArray();
    }

    public void Organize()
    {
        if (!Directory.Exists(_settings.SourceDirectory))
        {
            Console.WriteLine($"Source directory '{_settings.SourceDirectory}' does not exist.");
            return;
        }

        Directory.CreateDirectory(_settings.DestinationRoot);
        ProcessDirectory(_settings.SourceDirectory);
        CleanEmptyDirectories(_settings.SourceDirectory);
    }

    private void ProcessDirectory(string directory)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            ProcessFile(file);
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            if (_distributionDetector.IsDistributionDirectory(subDirectory))
            {
                MoveDistributionDirectory(subDirectory);
                continue;
            }

            ProcessDirectory(subDirectory);

            if (IsDirectoryEmpty(subDirectory))
            {
                Directory.Delete(subDirectory, recursive: false);
            }
        }
    }

    private void ProcessFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (extension == ".lnk")
        {
            File.Delete(filePath);
            return;
        }

        var category = _categories.FirstOrDefault(c => c.Matches(extension));
        if (category is null)
        {
            return;
        }

        switch (category)
        {
            case ImagesCategory:
                _photoService.MovePhoto(filePath, _settings.DestinationRoot, _settings.ImagesFolderName);
                break;
            case ArchivesCategory:
                _archiveService.HandleArchive(filePath, _settings.DestinationRoot, _settings.ArchiveFolderName, _settings.SoftFolderName);
                break;
            case SoftCategory:
                FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName));
                break;
            default:
                FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, category.FolderName));
                break;
        }
    }

    private void MoveDistributionDirectory(string directory)
    {
        var softRoot = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName);
        Directory.CreateDirectory(softRoot);

        var destinationPath = GetUniqueDirectoryPath(softRoot, Path.GetFileName(directory) ?? "Distribution");
        Directory.Move(directory, destinationPath);
    }

    private static string GetUniqueDirectoryPath(string root, string directoryName)
    {
        var destinationPath = Path.Combine(root, directoryName);
        if (!Directory.Exists(destinationPath))
        {
            return destinationPath;
        }

        var counter = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(root, $"{directoryName}({counter})");
            counter++;
        } while (Directory.Exists(candidate));

        return candidate;
    }

    private void CleanEmptyDirectories(string root)
    {
        foreach (var directory in Directory.GetDirectories(root))
        {
            CleanEmptyDirectories(directory);
            if (IsDirectoryEmpty(directory))
            {
                Directory.Delete(directory, recursive: false);
            }
        }
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }
}
