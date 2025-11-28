using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class DownloadsCleanupService : IDownloadsCleanupService
{
    private readonly IMessageWriter _messageWriter;

    public DownloadsCleanupService(IMessageWriter messageWriter)
    {
        _messageWriter = messageWriter;
    }

    public void CleanDownloadsIfRequested(string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return;
        }

        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfilePath))
        {
            return;
        }

        var downloadsPath = Path.Combine(userProfilePath, "Downloads");
        if (!Directory.Exists(downloadsPath))
        {
            return;
        }

        var normalizedDownloadsPath = NormalizePath(downloadsPath);
        var normalizedDestinationPath = NormalizePath(destinationPath);

        if (string.Equals(normalizedDownloadsPath, normalizedDestinationPath, StringComparison.OrdinalIgnoreCase))
        {
            _messageWriter.WriteLine("Downloads folder is already used as the source. No cleanup required.");
            _messageWriter.WriteLine(string.Empty);
            return;
        }

        _messageWriter.WriteLine(
            $"Move all files from \"{downloadsPath}\" to \"{destinationPath}\" for sorting? (y/n)");

        var key = Console.ReadKey(intercept: true).KeyChar;
        _messageWriter.WriteLine(string.Empty);

        if (char.ToLowerInvariant(key) != 'y')
        {
            _messageWriter.WriteLine("Downloads folder cleanup skipped.");
            _messageWriter.WriteLine(string.Empty);
            return;
        }

        Directory.CreateDirectory(destinationPath);

        foreach (var entry in new DirectoryInfo(downloadsPath).EnumerateFileSystemInfos())
        {
            var normalizedEntryPath = NormalizePath(entry.FullName);

            if (string.Equals(normalizedEntryPath, normalizedDestinationPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destination = EnsureUniquePath(Path.Combine(destinationPath, entry.Name));
            MoveEntry(entry, destination);
        }

        _messageWriter.WriteLine("Downloads folder cleaned.");
        _messageWriter.WriteLine(string.Empty);
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string EnsureUniquePath(string destinationPath)
    {
        if (!File.Exists(destinationPath) && !Directory.Exists(destinationPath))
        {
            return destinationPath;
        }

        var directory = Path.GetDirectoryName(destinationPath) ?? string.Empty;
        var baseName = Path.GetFileNameWithoutExtension(destinationPath);
        var extension = Path.GetExtension(destinationPath);
        var counter = 1;

        string candidate;
        do
        {
            candidate = Path.Combine(directory, $"{baseName} ({counter}){extension}");
            counter++;
        }
        while (File.Exists(candidate) || Directory.Exists(candidate));

        return candidate;
    }

    private static void MoveEntry(FileSystemInfo entry, string destinationPath)
    {
        if (entry is DirectoryInfo)
        {
            Directory.Move(entry.FullName, destinationPath);
            return;
        }

        File.Move(entry.FullName, destinationPath);
    }
}
