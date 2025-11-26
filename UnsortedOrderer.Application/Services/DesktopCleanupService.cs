using System.Collections.Generic;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class DesktopCleanupService : IDesktopCleanupService
{
    private static readonly HashSet<string> ShortcutExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".lnk",
        ".url",
    };

    private readonly IMessageWriter _messageWriter;

    public DesktopCleanupService(IMessageWriter messageWriter)
    {
        _messageWriter = messageWriter;
    }

    public void CleanIfRunningFromDesktop(string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return;
        }

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            return;
        }

        var normalizedDesktopPath = NormalizePath(desktopPath);
        var normalizedAppPath = NormalizePath(AppContext.BaseDirectory);

        if (!IsRunningFromDesktop(normalizedDesktopPath, normalizedAppPath))
        {
            return;
        }

        _messageWriter.WriteLine(
            $"Программа запущена с рабочего стола. Переместить все файлы (кроме ярлыков программ) в папку \"{destinationPath}\"? (y/n)");

        var key = Console.ReadKey(intercept: true).KeyChar;
        _messageWriter.WriteLine(string.Empty);

        if (char.ToLowerInvariant(key) != 'y')
        {
            _messageWriter.WriteLine("Очистка рабочего стола пропущена.");
            _messageWriter.WriteLine(string.Empty);
            return;
        }

        Directory.CreateDirectory(destinationPath);
        var currentProcessPath = Environment.ProcessPath;
        var normalizedAppSettingsPath = NormalizePath(Path.Combine(normalizedAppPath, "appsettings.json"));

        foreach (var entry in new DirectoryInfo(desktopPath).EnumerateFileSystemInfos())
        {
            var normalizedEntryPath = NormalizePath(entry.FullName);

            if (IsShortcut(entry) ||
                IsCurrentProcess(normalizedEntryPath, currentProcessPath) ||
                IsAppSettings(normalizedEntryPath, normalizedAppSettingsPath) ||
                IsAppBaseDirectory(normalizedEntryPath, normalizedAppPath))
            {
                continue;
            }

            var destination = EnsureUniquePath(Path.Combine(destinationPath, entry.Name));
            MoveEntry(entry, destination);
        }

        _messageWriter.WriteLine("Рабочий стол очищен.");
        _messageWriter.WriteLine("Нажмите любую клавишу для запуска основной программы.");
        Console.ReadKey(intercept: true);
        _messageWriter.WriteLine(string.Empty);
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool IsRunningFromDesktop(string normalizedDesktopPath, string normalizedAppPath)
    {
        if (string.Equals(normalizedDesktopPath, normalizedAppPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedAppPath.StartsWith(
            normalizedDesktopPath + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShortcut(FileSystemInfo entry)
    {
        return entry is FileInfo fileInfo && ShortcutExtensions.Contains(fileInfo.Extension);
    }

    private static bool IsCurrentProcess(string normalizedEntryPath, string? currentProcessPath)
    {
        if (string.IsNullOrWhiteSpace(currentProcessPath))
        {
            return false;
        }

        return string.Equals(
            normalizedEntryPath,
            NormalizePath(currentProcessPath),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAppSettings(string normalizedEntryPath, string normalizedAppSettingsPath)
    {
        return string.Equals(normalizedEntryPath, normalizedAppSettingsPath, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAppBaseDirectory(string normalizedEntryPath, string normalizedAppDirectory)
    {
        return string.Equals(normalizedEntryPath, normalizedAppDirectory, StringComparison.OrdinalIgnoreCase);
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
