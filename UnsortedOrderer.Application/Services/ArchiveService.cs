using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class ArchiveService : IArchiveService
{
    private readonly AppSettings _settings;

    public ArchiveService(AppSettings settings)
    {
        _settings = settings;
    }

    public string HandleArchive(string archivePath, string destinationDirectory)
    {
        var finalDestinationDirectory = FindMatchingDestinationDirectory(archivePath)
            ?? destinationDirectory;

        Directory.CreateDirectory(finalDestinationDirectory);

        var destinationPath = Path.Combine(finalDestinationDirectory, Path.GetFileName(archivePath));
        var finalPath = FileUtilities.GetUniqueFilePath(destinationPath);
        File.Move(archivePath, finalPath);

        return finalPath;
    }

    private string? FindMatchingDestinationDirectory(string archivePath)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath);
        if (string.IsNullOrWhiteSpace(archiveName))
        {
            return null;
        }

        var sourceDirectory = Path.GetDirectoryName(archivePath);
        if (!string.IsNullOrWhiteSpace(sourceDirectory))
        {
            var sourceSiblingDestination = FindMatchingSiblingDirectory(sourceDirectory, archivePath, archiveName);
            if (sourceSiblingDestination is not null)
            {
                return sourceSiblingDestination;
            }
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

    private static string? FindMatchingSiblingDirectory(string sourceDirectory, string archivePath, string archiveName)
    {
        foreach (var siblingPath in Directory.EnumerateFileSystemEntries(sourceDirectory))
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

            return Directory.Exists(siblingPath)
                ? siblingPath
                : Path.GetDirectoryName(siblingPath);
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
