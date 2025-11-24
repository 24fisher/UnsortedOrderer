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
    private readonly Dictionary<string, int> _movedFilesByCategory = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _deletedDirectories = new(StringComparer.OrdinalIgnoreCase);
    private int _totalMovedFiles;

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
        Console.WriteLine($"Ensured destination root exists at '{_settings.DestinationRoot}'.");

        Console.WriteLine("Press any key to start file scan...");
        Console.ReadKey(intercept: true);

        ProcessDirectory(_settings.SourceDirectory);
        CleanEmptyDirectories(_settings.SourceDirectory);

        PrintStatistics();
    }

    private void ProcessDirectory(string directory)
    {
        Console.WriteLine($"Scanning directory: {directory}");

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
                RecordDeletedDirectory(subDirectory);
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
                var photoDestination = _photoService.MovePhoto(filePath, _settings.DestinationRoot, _settings.ImagesFolderName);
                RecordMovedFile(photoDestination, category.FolderName);
                break;
            case ArchivesCategory:
                HandleArchiveFile(filePath, category.FolderName);
                break;
            case SoftCategory:
                var softDestination = FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName));
                RecordMovedFile(softDestination, category.FolderName);
                break;
            default:
                var destination = FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, category.FolderName));
                RecordMovedFile(destination, category.FolderName);
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

    private void HandleArchiveFile(string filePath, string categoryName)
    {
        var archiveName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        var potentialDistributionDirectory = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName, archiveName);
        if (Directory.Exists(potentialDistributionDirectory))
        {
            RecordDeletedDirectory(potentialDistributionDirectory);
            Directory.Delete(potentialDistributionDirectory, recursive: true);
        }

        var archiveDestination = _archiveService.HandleArchive(filePath, _settings.DestinationRoot, _settings.ArchiveFolderName, _settings.SoftFolderName);
        RecordMovedFile(archiveDestination, categoryName);
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
                RecordDeletedDirectory(directory);
                Directory.Delete(directory, recursive: false);
            }
        }
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private void RecordMovedFile(string destinationPath, string category)
    {
        _totalMovedFiles++;
        if (_movedFilesByCategory.ContainsKey(category))
        {
            _movedFilesByCategory[category]++;
        }
        else
        {
            _movedFilesByCategory[category] = 1;
        }

        Console.WriteLine($"Moved to '{destinationPath}' (category: {category}).");
    }

    private void RecordDeletedDirectory(string directory)
    {
        if (_deletedDirectories.Add(directory))
        {
            Console.WriteLine($"Deleted directory: {directory}");
        }
    }

    private void PrintStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("Organization summary:");
        Console.WriteLine($"Total files moved: {_totalMovedFiles}");

        foreach (var category in _movedFilesByCategory.OrderBy(c => c.Key))
        {
            Console.WriteLine($"  {category.Key}: {category.Value}");
        }

        if (_deletedDirectories.Count == 0)
        {
            Console.WriteLine("Deleted directories: none");
            return;
        }

        Console.WriteLine("Deleted directories:");
        foreach (var directory in _deletedDirectories.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  {directory}");
        }
    }
}
