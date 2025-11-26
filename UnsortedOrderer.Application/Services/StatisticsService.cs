using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly IMessageWriter _messageWriter;
    private readonly IReadOnlyCollection<IFileCategory> _categories;
    private readonly Dictionary<string, int> _movedFilesByCategory;
    private readonly HashSet<string> _deletedDirectories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _unknownExtensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _deletedFilesByExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<NonSplittableDirectoryRecord> _movedNonSplittableDirectories = new();
    private int _totalMovedFiles;
    private int _totalUnknownFiles;
    private int _totalDeletedFiles;

    public StatisticsService(IEnumerable<IFileCategory> categories, IMessageWriter messageWriter)
    {
        _categories = categories.ToArray();
        _messageWriter = messageWriter;
        _movedFilesByCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in _categories)
        {
            _movedFilesByCategory[category.FolderName] = 0;
        }
    }

    public void RecordMovedFile(string destinationPath, string category)
    {
        RecordMovedFiles(1, category);

        _messageWriter.WriteLine($"Moved to '{destinationPath}' (category: {category}).");
    }

    public void RecordMovedFiles(int count, string category)
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

    public void RecordMovedNonSplittableDirectory(INonSplittableDirectoryCategory category, string destinationPath, int fileCount)
    {
        _movedNonSplittableDirectories.Add(new NonSplittableDirectoryRecord(destinationPath, category.FolderName, fileCount));
        RecordMovedFiles(fileCount, category.FolderName);

        _messageWriter.WriteLine($"Moved directory '{destinationPath}' (category: {category.FolderName}, files: {fileCount}).");
    }

    public void RecordDeletedDirectory(string directory)
    {
        if (_deletedDirectories.Add(directory))
        {
            _messageWriter.WriteLine($"Deleted directory: {directory}");
        }
    }

    public void RecordUnknownFile(string extension)
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

    public void RecordDeletedFile(string extension)
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

    public void PrintStatistics(string sourceDirectory)
    {
        _messageWriter.WriteLine(string.Empty);
        _messageWriter.WriteLine("Organization summary:");
        _messageWriter.WriteLine($"Total files moved: {_totalMovedFiles}");

        foreach (var category in _categories.OrderBy(c => c.FolderName, StringComparer.OrdinalIgnoreCase))
        {
            var movedCount = _movedFilesByCategory.TryGetValue(category.FolderName, out var count)
                ? count
                : 0;
            _messageWriter.WriteLine($"  {category.FolderName}: {movedCount}");
        }

        if (_unknownExtensions.Count == 0)
        {
            _messageWriter.WriteLine("Unknown file types: none");
        }
        else
        {
            _messageWriter.WriteLine($"Unknown file types encountered: {_totalUnknownFiles}");
            foreach (var unknown in _unknownExtensions.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                _messageWriter.WriteLine($"  {unknown.Key}: {unknown.Value}");
            }
        }

        PrintDeletedFilesByExtension();
        PrintNonSplittableDirectories();
        PrintDeletedDirectories();
        PrintSourceDirectoryCompletion(sourceDirectory);
    }

    private void PrintDeletedDirectories()
    {
        if (_deletedDirectories.Count == 0)
        {
            _messageWriter.WriteLine("Deleted directories: none");
            return;
        }

        _messageWriter.WriteLine("Deleted directories:");
        foreach (var directory in _deletedDirectories.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            _messageWriter.WriteLine($"  {directory}");
        }
    }

    private void PrintDeletedFilesByExtension()
    {
        var filteredDeleted = _deletedFilesByExtension
            .Where(kvp => !kvp.Key.Equals(".tmp", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (_totalDeletedFiles == 0 || filteredDeleted.Length == 0)
        {
            _messageWriter.WriteLine("Deleted uncategorized files: none");
            return;
        }

        _messageWriter.WriteLine($"Deleted uncategorized files: {filteredDeleted.Sum(kvp => kvp.Value)}");
        foreach (var deleted in filteredDeleted.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            _messageWriter.WriteLine($"  {deleted.Key}: {deleted.Value}");
        }
    }

    private void PrintSourceDirectoryCompletion(string sourceDirectory)
    {
        if (!IsDirectoryEmpty(sourceDirectory))
        {
            return;
        }

        _messageWriter.WriteLine(string.Empty);
        _messageWriter.WriteLine("====================");
        _messageWriter.WriteLine("Source directory fully processed. No files remain to organize.");
        _messageWriter.WriteLine("====================");
    }

    private void PrintNonSplittableDirectories()
    {
        if (_movedNonSplittableDirectories.Count == 0)
        {
            _messageWriter.WriteLine("Non-splittable directories moved: none");
            return;
        }

        _messageWriter.WriteLine("Non-splittable directories moved:");
        foreach (var record in _movedNonSplittableDirectories
                     .OrderBy(r => r.Destination, StringComparer.OrdinalIgnoreCase))
        {
            _messageWriter.WriteLine($"  {record.Destination} ({record.FileCount} files)");
        }
    }

    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private sealed record NonSplittableDirectoryRecord(string Destination, string Category, int FileCount);
}
