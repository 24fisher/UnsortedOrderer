using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnsortedOrderer.Application.Contracts.Services.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Services.Categories;

public sealed class ArchiveService : IArchiveService
{
    private readonly AppSettings _settings;
    private readonly IReadOnlyCollection<ICategory> _categories;
    private readonly IReadOnlyCollection<INonSplittableDirectoryCategory> _nonSplittableCategories;
    private readonly IStatisticsService _statisticsService;
    private readonly string[] _softwareArchiveKeywords;

    public ArchiveService(
        AppSettings settings,
        IEnumerable<ICategory> categories,
        IStatisticsService statisticsService)
    {
        _settings = settings;
        _categories = categories.ToArray();
        _nonSplittableCategories = _categories
            .OfType<INonSplittableDirectoryCategory>()
            .ToArray();
        _statisticsService = statisticsService;
        _softwareArchiveKeywords = settings.SoftwareArchiveKeywords ?? Array.Empty<string>();
    }

    public ArchiveHandlingResult HandleArchiveFile(string filePath, string categoryName)
    {
        RemoveExistingDistributionDirectory(filePath);

        if (IsSoftwareArchive(filePath))
        {
            var softArchiveDestination = Path.Combine(_settings.DestinationRoot, _settings.SoftFolderName);
            var movedArchive = HandleArchive(filePath, softArchiveDestination);
            return new ArchiveHandlingResult(movedArchive, _settings.SoftFolderName);
        }

        var matchingCategory = FindMatchingSiblingCategory(filePath);
        if (matchingCategory is not null)
        {
            var matchingDestinationDirectory = Path.Combine(_settings.DestinationRoot, matchingCategory.FolderName);
            var matchingArchiveDestination = HandleArchive(filePath, matchingDestinationDirectory);
            return new ArchiveHandlingResult(matchingArchiveDestination, matchingCategory.FolderName);
        }

        var archiveDestinationDirectory = Path.Combine(_settings.DestinationRoot, _settings.ArchiveFolderName);
        var archiveDestination = HandleArchive(filePath, archiveDestinationDirectory);
        return new ArchiveHandlingResult(archiveDestination, categoryName);
    }

    public string HandleArchive(string archivePath, string destinationDirectory)
    {
        var finalDestinationDirectory = FindMatchingDestinationDirectory(archivePath)
            ?? destinationDirectory;

        Directory.CreateDirectory(finalDestinationDirectory);

        var destinationPath = Path.Combine(finalDestinationDirectory, Path.GetFileName(archivePath));
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        File.Move(archivePath, destinationPath);

        return destinationPath;
    }

    private void RemoveExistingDistributionDirectory(string archivePath)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath) ?? string.Empty;
        var potentialDistributionDirectory = Path.Combine(
            _settings.DestinationRoot,
            _settings.SoftFolderName,
            archiveName);

        if (Directory.Exists(potentialDistributionDirectory))
        {
            _statisticsService.RecordDeletedDirectory(potentialDistributionDirectory);
            Directory.Delete(potentialDistributionDirectory, recursive: true);
        }
    }

    private ICategory? FindMatchingSiblingCategory(string archivePath)
    {
        var directory = Path.GetDirectoryName(archivePath);
        var archiveName = Path.GetFileNameWithoutExtension(archivePath);

        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(archiveName))
        {
            return null;
        }

        foreach (var siblingPath in Directory.EnumerateFileSystemEntries(directory))
        {
            if (string.Equals(siblingPath, archivePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var siblingName = Directory.Exists(siblingPath)
                ? Path.GetFileName(siblingPath)
                : Path.GetFileNameWithoutExtension(siblingPath);

            if (!IsHalfNameMatch(archiveName, siblingName))
            {
                continue;
            }

            if (Directory.Exists(siblingPath))
            {
                var siblingCategory = _nonSplittableCategories
                    .FirstOrDefault(category => category.IsNonSplittableDirectory(siblingPath));

                if (siblingCategory is not null)
                {
                    return siblingCategory;
                }

                continue;
            }

            var matchedExtension = Path.GetExtension(siblingPath).ToLowerInvariant();
            var category = _categories.FirstOrDefault(c => c.Matches(matchedExtension));
            if (category is not null)
            {
                return category;
            }
        }

        return null;
    }

    private bool IsSoftwareArchive(string archivePath)
    {
        if (_softwareArchiveKeywords.Length == 0)
        {
            return false;
        }

        var archiveName = Path.GetFileNameWithoutExtension(archivePath) ?? string.Empty;
        return _softwareArchiveKeywords.Any(keyword =>
            archiveName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private string? FindMatchingDestinationDirectory(string archivePath)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath);
        if (string.IsNullOrWhiteSpace(archiveName))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_settings.DestinationRoot) && Directory.Exists(_settings.DestinationRoot))
        {
            var destinationSiblingDirectory = FindMatchingDirectoryInDestinationRoot(archiveName);
            if (destinationSiblingDirectory is not null)
            {
                return destinationSiblingDirectory;
            }
        }

        return null;
    }

    private string? FindMatchingDirectoryInDestinationRoot(string archiveName)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(_settings.DestinationRoot, "*", SearchOption.AllDirectories))
        {
            var siblingName = Directory.Exists(entry)
                ? Path.GetFileName(entry)
                : Path.GetFileNameWithoutExtension(entry);

            if (!IsHalfNameMatch(archiveName, siblingName))
            {
                continue;
            }

            return Directory.Exists(entry)
                ? entry
                : Path.GetDirectoryName(entry);
        }

        return null;
    }

    private static bool IsHalfNameMatch(string archiveName, string siblingName)
    {
        if (string.IsNullOrWhiteSpace(archiveName) || string.IsNullOrWhiteSpace(siblingName))
        {
            return false;
        }

        var compareLength = Math.Min(archiveName.Length, siblingName.Length) / 2;
        if (compareLength == 0)
        {
            return false;
        }

        return archiveName
            .AsSpan(0, compareLength)
            .Equals(siblingName.AsSpan(0, compareLength), StringComparison.OrdinalIgnoreCase);
    }
}
