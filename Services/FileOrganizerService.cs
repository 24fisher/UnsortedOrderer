using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class FileOrganizerService
{
    private readonly AppSettings _settings;
    private readonly IArchiveService _archiveService;
    private readonly IPhotoService _photoService;
    private readonly IReadOnlyCollection<IFileCategory> _categories;
    private readonly IReadOnlyCollection<INonSplittableDirectoryCategory> _nonSplittableCategories;
    private readonly IReadOnlyCollection<ImageCategoryBase> _imageCategories;
    private readonly DocumentsCategory? _documentsCategory;
    private readonly DriversCategory? _driversCategory;
    private readonly PhotosCategory? _photosCategory;
    private readonly ImagesCategory? _imagesCategory;
    private readonly CategoryCache _categoryCache;
    private readonly UnknownCategory _unknownCategory;
    private readonly SpecialCategoriesHandler _specialCategoriesHandler;
    private readonly Dictionary<string, int> _movedFilesByCategory = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _deletedDirectories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _unknownExtensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _deletedFilesByExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _deletedExtensions;
    private readonly List<NonSplittableDirectoryRecord> _movedNonSplittableDirectories = new();
    private int _totalMovedFiles;
    private int _totalUnknownFiles;
    private int _totalDeletedFiles;

    public FileOrganizerService(
        AppSettings settings,
        IArchiveService archiveService,
        IPhotoService photoService,
        IEnumerable<IFileCategory> categories)
    {
        _settings = settings;
        _archiveService = archiveService;
        _photoService = photoService;
        _categories = categories.ToArray();
        _nonSplittableCategories = _categories.OfType<INonSplittableDirectoryCategory>().ToArray();
        _imageCategories = _categories.OfType<ImageCategoryBase>().ToArray();
        _documentsCategory = _categories.OfType<DocumentsCategory>().FirstOrDefault();
        _driversCategory = _categories.OfType<DriversCategory>().FirstOrDefault();
        _photosCategory = _imageCategories.OfType<PhotosCategory>().FirstOrDefault();
        _imagesCategory = _imageCategories.OfType<ImagesCategory>().FirstOrDefault();
        _categoryCache = new CategoryCache(_categories);
        _specialCategoriesHandler = new SpecialCategoriesHandler(
            _imageCategories,
            _documentsCategory,
            _categoryCache,
            _settings.DocumentImageKeywords);
        _deletedExtensions = FileUtilities
            .NormalizeExtensions(settings.DeletedExtensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
            var nonSplittableCategory = _nonSplittableCategories
                .FirstOrDefault(category => category.IsNonSplittableDirectory(subDirectory));

            if (nonSplittableCategory is not null)
            {
                MoveNonSplittableDirectory(subDirectory, nonSplittableCategory);
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

        if (_deletedExtensions.Contains(extension))
        {
            RecordDeletedFile(extension);
            File.Delete(filePath);
            return;
        }

        var category = _specialCategoriesHandler.TryGetSpecialCategory(filePath, extension)
            ?? ResolveCategory(filePath, extension);
        if (category is null)
        {
            RecordUnknownFile(extension);
            MoveToUnknown(filePath, extension);
            return;
        }

        switch (category)
        {
            case PhotosCategory:
                var photoDestination = _photoService.MovePhoto(filePath, _settings.DestinationRoot, _settings.PhotosFolderName);
                RecordMovedFile(photoDestination, category.FolderName);
                break;
            case ImagesCategory:
                var imageDestination = FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, category.FolderName));
                RecordMovedFile(imageDestination, category.FolderName);
                break;
            case ArchivesCategory:
                HandleArchiveFile(filePath, category.FolderName);
                break;
            default:
                var destinationDirectory = category is INonSplittableDirectoryCategory nonSplittableCategory
                    ? nonSplittableCategory.GetFileDestination(_settings.DestinationRoot, filePath)
                    : Path.Combine(_settings.DestinationRoot, category.FolderName);
                var destination = FileUtilities.MoveFile(filePath, destinationDirectory);
                RecordMovedFile(destination, category.FolderName);
                break;
        }
    }

    private IFileCategory? ResolveCategory(string filePath, string extension)
    {
        if (_driversCategory is not null && _driversCategory.IsDriverFile(filePath))
        {
            return _driversCategory;
        }

        if (_categoryCache.ImageCategoryExtensions.Contains(extension))
        {
            if (_photosCategory is null || _imagesCategory is null)
            {
                return _imageCategories.FirstOrDefault(category => category.Matches(extension));
            }

            return _photoService.IsPhoto(filePath)
                ? _photosCategory
                : _imagesCategory;
        }

        return _categoryCache.CategoriesByExtension.TryGetValue(extension, out var category)
            ? category
            : null;
    }

    private void MoveNonSplittableDirectory(string directory, INonSplittableDirectoryCategory category)
    {
        var destinationPath = category.GetDirectoryDestination(_settings.DestinationRoot, directory);
        var fileCount = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Count();
        Directory.Move(directory, destinationPath);

        RecordMovedNonSplittableDirectory(category, destinationPath, fileCount);
    }

    private void HandleArchiveFile(string filePath, string categoryName)
    {
        RemoveExistingDistributionDirectory(filePath);

        var matchingCategory = FindMatchingSiblingCategory(filePath);
        if (matchingCategory is not null)
        {
            var matchingDestinationDirectory = Path.Combine(_settings.DestinationRoot, matchingCategory.FolderName);
            var matchingArchiveDestination = _archiveService.HandleArchive(filePath, matchingDestinationDirectory);
            RecordMovedFile(matchingArchiveDestination, matchingCategory.FolderName);
            return;
        }

        var archiveDestinationDirectory = Path.Combine(_settings.DestinationRoot, _settings.ArchiveFolderName);
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
        RecordMovedFiles(1, category);

        Console.WriteLine($"Moved to '{destinationPath}' (category: {category}).");
    }

    private void RecordMovedFiles(int count, string category)
    {
        _totalMovedFiles += count;
        if (_movedFilesByCategory.ContainsKey(category))
        {
            _movedFilesByCategory[category] += count;
        }
        else
        {
            _movedFilesByCategory[category] = count;
        }
    }

    private void RecordMovedNonSplittableDirectory(
        INonSplittableDirectoryCategory category,
        string destinationPath,
        int fileCount)
    {
        _movedNonSplittableDirectories.Add(new NonSplittableDirectoryRecord(destinationPath, category.FolderName, fileCount));
        RecordMovedFiles(fileCount, category.FolderName);

        Console.WriteLine($"Moved directory '{destinationPath}' (category: {category.FolderName}, files: {fileCount}).");
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

    private void RecordDeletedFile(string extension)
    {
        _totalDeletedFiles++;
        var key = string.IsNullOrWhiteSpace(extension) ? "(no extension)" : extension;

        if (_deletedFilesByExtension.ContainsKey(key))
        {
            _deletedFilesByExtension[key]++;
        }
        else
        {
            _deletedFilesByExtension[key] = 1;
        }
    }

    private void ValidateCategoryExtensions()
    {
        var extensionToCategories = new Dictionary<string, List<IFileCategory>>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in _categories)
        {
            foreach (var extension in category.Extensions)
            {
                if (!extensionToCategories.TryGetValue(extension, out var categories))
                {
                    categories = new List<IFileCategory>();
                    extensionToCategories[extension] = categories;
                }

                categories.Add(category);
            }
        }

        var overlaps = extensionToCategories
            .Where(kvp => kvp.Value.Count > 1)
            .Where(kvp => !kvp.Value.All(category => category is ImageCategoryBase))
            .ToArray();

        if (overlaps.Length == 0)
        {
            return;
        }

        var overlapDetails = overlaps
            .Select(kvp =>
            {
                var names = kvp.Value
                    .Select(c => c.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase);

                return $"{kvp.Key}: {string.Join(", ", names)}";
            });

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

        PrintDeletedFilesByExtension();

        PrintNonSplittableDirectories();

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

        PrintSourceDirectoryCompletion();
    }

    private void PrintDeletedFilesByExtension()
    {
        var filteredDeleted = _deletedFilesByExtension
            .Where(kvp => !kvp.Key.Equals(".tmp", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (_totalDeletedFiles == 0 || filteredDeleted.Length == 0)
        {
            Console.WriteLine("Deleted uncategorized files: none");
            return;
        }

        Console.WriteLine($"Deleted uncategorized files: {filteredDeleted.Sum(kvp => kvp.Value)}");
        foreach (var deleted in filteredDeleted.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  {deleted.Key}: {deleted.Value}");
        }
    }

    private void PrintSourceDirectoryCompletion()
    {
        if (!IsDirectoryEmpty(_settings.SourceDirectory))
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("====================");
        Console.WriteLine("Source directory fully processed. No files remain to organize.");
        Console.WriteLine("====================");
    }

    private void PrintNonSplittableDirectories()
    {
        if (_movedNonSplittableDirectories.Count == 0)
        {
            Console.WriteLine("Non-splittable directories moved: none");
            return;
        }

        Console.WriteLine("Non-splittable directories moved:");
        foreach (var record in _movedNonSplittableDirectories
                     .OrderBy(r => r.Destination, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  {record.Destination} ({record.FileCount} files)");
        }
    }

    private sealed record NonSplittableDirectoryRecord(string Destination, string Category, int FileCount);
}
