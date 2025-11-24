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
    private readonly UnknownCategory _unknownCategory;
    private static readonly string[] DocumentImageKeywords = new[] { "скан", "паспорт", "свидетельство", "документ" };
    private readonly Dictionary<string, int> _movedFilesByCategory = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _deletedDirectories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _unknownExtensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _deletedUncategorizedFiles = new(StringComparer.OrdinalIgnoreCase);
    private int _totalMovedFiles;
    private int _totalUnknownFiles;
    private int _totalDeletedUncategorizedFiles;

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

        _unknownCategory = _categories.OfType<UnknownCategory>().FirstOrDefault()
            ?? throw new InvalidOperationException("Unknown category is missing.");

        ValidateCategoryExtensions();

        foreach (var category in _categories)
        {
            _movedFilesByCategory[category.FolderName] = 0;
        }
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
            RecordDeletedUncategorizedFile(extension);
            File.Delete(filePath);
            return;
        }

        var category = TryGetSpecialCategory(filePath, extension)
            ?? _categories.FirstOrDefault(c => c.Matches(extension));
        if (category is null)
        {
            RecordUnknownFile(extension);
            MoveToUnknown(filePath, extension);
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
                var softDestinationDirectory = GetDistributionDestinationDirectory(
                    Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName),
                    filePath);
                var softDestination = FileUtilities.MoveFile(filePath, softDestinationDirectory);
                RecordMovedFile(softDestination, category.FolderName);
                break;
            default:
                var destination = FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, category.FolderName));
                RecordMovedFile(destination, category.FolderName);
                break;
        }
    }

    private IFileCategory? TryGetSpecialCategory(string filePath, string extension)
    {
        var imagesCategory = _categories.OfType<ImagesCategory>().FirstOrDefault();
        var documentsCategory = _categories.OfType<DocumentsCategory>().FirstOrDefault();

        if (imagesCategory is null || documentsCategory is null)
        {
            return null;
        }

        if (!imagesCategory.Matches(extension))
        {
            return null;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var lowerInvariantName = fileName.ToLowerInvariant();
        return DocumentImageKeywords.Any(keyword => lowerInvariantName.Contains(keyword))
            ? documentsCategory
            : null;
    }

    private void MoveDistributionDirectory(string directory)
    {
        var softRoot = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName);
        Directory.CreateDirectory(softRoot);

        var destinationPath = FileUtilities.GetUniqueDirectoryPath(softRoot, Path.GetFileName(directory) ?? "Distribution");
        Directory.Move(directory, destinationPath);
    }

    private void HandleArchiveFile(string filePath, string categoryName)
    {
        RemoveExistingDistributionDirectory(filePath);

        var matchingCategory = FindMatchingSiblingCategory(filePath);
        if (matchingCategory is not null)
        {
            var matchingDestinationDirectory = matchingCategory switch
            {
                SoftCategory => GetDistributionDestinationDirectory(
                    Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName),
                    filePath),
                ArchivesCategory => GetDistributionDestinationDirectory(
                    Path.Combine(_settings.DestinationRoot, _settings.ArchiveFolderName),
                    filePath),
                _ => Path.Combine(_settings.DestinationRoot, matchingCategory.FolderName)
            };

            var matchingArchiveDestination = _archiveService.HandleArchive(filePath, matchingDestinationDirectory);
            RecordMovedFile(matchingArchiveDestination, matchingCategory.FolderName);
            return;
        }

        var archiveDestinationDirectory = GetDistributionDestinationDirectory(
            Path.Combine(_settings.DestinationRoot, _settings.ArchiveFolderName),
            filePath);
        var archiveDestination = _archiveService.HandleArchive(filePath, archiveDestinationDirectory);
        RecordMovedFile(archiveDestination, categoryName);
    }

    private void RemoveExistingDistributionDirectory(string archivePath)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath) ?? string.Empty;
        var potentialDistributionDirectory = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName, archiveName);
        if (Directory.Exists(potentialDistributionDirectory))
        {
            RecordDeletedDirectory(potentialDistributionDirectory);
            Directory.Delete(potentialDistributionDirectory, recursive: true);
        }
    }

    private IFileCategory? FindMatchingSiblingCategory(string archivePath)
    {
        var directory = Path.GetDirectoryName(archivePath);
        var archiveName = Path.GetFileNameWithoutExtension(archivePath);

        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(archiveName))
        {
            return null;
        }

        var matchingFile = Directory
            .EnumerateFiles(directory)
            .FirstOrDefault(path => !string.Equals(path, archivePath, StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(
                                        Path.GetFileNameWithoutExtension(path),
                                        archiveName,
                                        StringComparison.OrdinalIgnoreCase));

        if (matchingFile is null)
        {
            return null;
        }

        var matchedExtension = Path.GetExtension(matchingFile).ToLowerInvariant();
        return _categories.FirstOrDefault(c => c.Matches(matchedExtension));
    }

    private static string GetDistributionDestinationDirectory(string categoryRoot, string filePath)
    {
        var programName = DistributionFolderHelper.TryGetProgramFolderName(filePath);
        return programName is null
            ? categoryRoot
            : Path.Combine(categoryRoot, programName);
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

    private void RecordUnknownFile(string extension)
    {
        _totalUnknownFiles++;
        var key = string.IsNullOrWhiteSpace(extension) ? "(no extension)" : extension;

        if (_unknownExtensions.ContainsKey(key))
        {
            _unknownExtensions[key]++;
        }
        else
        {
            _unknownExtensions[key] = 1;
        }
    }

    private void MoveToUnknown(string filePath, string extension)
    {
        var extensionFolderName = string.IsNullOrWhiteSpace(extension)
            ? "no-extension"
            : extension.TrimStart('.')
                .Replace(Path.DirectorySeparatorChar, '-')
                .Replace(Path.AltDirectorySeparatorChar, '-');

        var unknownDirectory = Path.Combine(_settings.DestinationRoot, _unknownCategory.FolderName, extensionFolderName);
        var destination = FileUtilities.MoveFile(filePath, unknownDirectory);
        RecordMovedFile(destination, _unknownCategory.FolderName);
    }

    private void RecordDeletedUncategorizedFile(string extension)
    {
        _totalDeletedUncategorizedFiles++;
        var key = string.IsNullOrWhiteSpace(extension) ? "(no extension)" : extension;

        if (_deletedUncategorizedFiles.ContainsKey(key))
        {
            _deletedUncategorizedFiles[key]++;
        }
        else
        {
            _deletedUncategorizedFiles[key] = 1;
        }
    }

    private void ValidateCategoryExtensions()
    {
        var extensionToCategories = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in _categories)
        {
            foreach (var extension in category.Extensions)
            {
                if (!extensionToCategories.TryGetValue(extension, out var categorySet))
                {
                    categorySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    extensionToCategories[extension] = categorySet;
                }

                categorySet.Add(category.Name);
            }
        }

        var overlaps = extensionToCategories
            .Where(kvp => kvp.Value.Count > 1)
            .ToArray();

        if (overlaps.Length == 0)
        {
            return;
        }

        var overlapDetails = overlaps
            .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.OrderBy(v => v, StringComparer.OrdinalIgnoreCase))}");

        throw new InvalidOperationException($"Category extensions overlap: {string.Join("; ", overlapDetails)}");
    }

    private void PrintStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("Organization summary:");
        Console.WriteLine($"Total files moved: {_totalMovedFiles}");

        foreach (var category in _categories.OrderBy(c => c.FolderName, StringComparer.OrdinalIgnoreCase))
        {
            var movedCount = _movedFilesByCategory.TryGetValue(category.FolderName, out var count)
                ? count
                : 0;
            Console.WriteLine($"  {category.FolderName}: {movedCount}");
        }

        if (_unknownExtensions.Count == 0)
        {
            Console.WriteLine("Unknown file types: none");
        }
        else
        {
            Console.WriteLine($"Unknown file types encountered: {_totalUnknownFiles}");
            foreach (var unknown in _unknownExtensions.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  {unknown.Key}: {unknown.Value}");
            }
        }

        if (_totalDeletedUncategorizedFiles == 0)
        {
            Console.WriteLine("Deleted uncategorized files: none");
        }
        else
        {
            Console.WriteLine($"Deleted uncategorized files: {_totalDeletedUncategorizedFiles}");
            foreach (var deleted in _deletedUncategorizedFiles.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  {deleted.Key}: {deleted.Value}");
            }
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
