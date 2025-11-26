using UnsortedOrderer.Cache;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class FileOrganizerService
{
    private readonly AppSettings _settings;
    private readonly IArchiveService _archiveService;
    private readonly IPhotoService _photoService;
    private readonly IMessageWriter _messageWriter;
    private readonly IStatisticsService _statisticsService;
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
    private readonly HashSet<string> _deletedExtensions;

    public FileOrganizerService(
        AppSettings settings,
        IArchiveService archiveService,
        IPhotoService photoService,
        IEnumerable<IFileCategory> categories,
        IStatisticsService statisticsService,
        IMessageWriter messageWriter)
    {
        _settings = settings;
        _archiveService = archiveService;
        _photoService = photoService;
        _statisticsService = statisticsService;
        _messageWriter = messageWriter;
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
    }

    public void Organize()
    {
        if (!Directory.Exists(_settings.SourceDirectory))
        {
            _messageWriter.WriteLine($"Source directory '{_settings.SourceDirectory}' does not exist.");
            return;
        }

        Directory.CreateDirectory(_settings.DestinationRoot);
        _messageWriter.WriteLine($"Ensured destination root exists at '{_settings.DestinationRoot}'.");

        _messageWriter.WriteLine("Press any key to start file scan...");
        Console.ReadKey(intercept: true);

        ProcessDirectory(_settings.SourceDirectory);
        CleanEmptyDirectories(_settings.SourceDirectory);

        PrintStatistics();
    }

    private void ProcessDirectory(string directory)
    {
        _messageWriter.WriteLine($"Scanning directory: {directory}");

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
                _statisticsService.RecordDeletedDirectory(subDirectory);
                Directory.Delete(subDirectory, recursive: false);
            }
        }
    }

    private void ProcessFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (_deletedExtensions.Contains(extension))
        {
            _statisticsService.RecordDeletedFile(extension);
            File.Delete(filePath);
            return;
        }

        var category = _specialCategoriesHandler.TryGetSpecialCategory(filePath, extension)
            ?? ResolveCategory(filePath, extension);
        if (category is null)
        {
            _statisticsService.RecordUnknownFile(extension);
            MoveToUnknown(filePath, extension);
            return;
        }

        switch (category)
        {
            case PhotosCategory:
                var photoDestination = _photoService.MovePhoto(
                    filePath,
                    _settings.DestinationRoot,
                    _settings.PhotosFolderName,
                    _settings.CameraFileNamePatterns);
                _statisticsService.RecordMovedFile(photoDestination, category.FolderName);
                break;
            case ImagesCategory:
                var imageDestination = FileUtilities.MoveFile(filePath, Path.Combine(_settings.DestinationRoot, category.FolderName));
                _statisticsService.RecordMovedFile(imageDestination, category.FolderName);
                break;
            case ArchivesCategory:
                HandleArchiveFile(filePath, category.FolderName);
                break;
            default:
                var destinationDirectory = category is INonSplittableDirectoryCategory nonSplittableCategory
                    ? nonSplittableCategory.GetFileDestination(_settings.DestinationRoot, filePath)
                    : Path.Combine(_settings.DestinationRoot, category.FolderName);
                var destination = FileUtilities.MoveFile(filePath, destinationDirectory);
                _statisticsService.RecordMovedFile(destination, category.FolderName);
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

        _statisticsService.RecordMovedNonSplittableDirectory(category, destinationPath, fileCount);
    }

    private void HandleArchiveFile(string filePath, string categoryName)
    {
        RemoveExistingDistributionDirectory(filePath);

        var matchingCategory = FindMatchingSiblingCategory(filePath);
        if (matchingCategory is not null)
        {
            var matchingDestinationDirectory = Path.Combine(_settings.DestinationRoot, matchingCategory.FolderName);
            var matchingArchiveDestination = _archiveService.HandleArchive(filePath, matchingDestinationDirectory);
            _statisticsService.RecordMovedFile(matchingArchiveDestination, matchingCategory.FolderName);
            return;
        }

        var archiveDestinationDirectory = Path.Combine(_settings.DestinationRoot, _settings.ArchiveFolderName);
        var archiveDestination = _archiveService.HandleArchive(filePath, archiveDestinationDirectory);
        _statisticsService.RecordMovedFile(archiveDestination, categoryName);
    }

    private void RemoveExistingDistributionDirectory(string archivePath)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath) ?? string.Empty;
        var potentialDistributionDirectory = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName, archiveName);
        if (Directory.Exists(potentialDistributionDirectory))
        {
            _statisticsService.RecordDeletedDirectory(potentialDistributionDirectory);
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
                _statisticsService.RecordDeletedDirectory(directory);
                Directory.Delete(directory, recursive: false);
            }
        }
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
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
        _statisticsService.RecordMovedFile(destination, _unknownCategory.FolderName);
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
        _statisticsService.PrintStatistics(_settings.SourceDirectory);
    }
}
