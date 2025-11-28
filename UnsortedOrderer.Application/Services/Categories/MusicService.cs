using System;
using System.Collections.Generic;
using System.IO;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Application.Contracts.Services.Categories;

namespace UnsortedOrderer.Application.Services.Categories;

public class MusicService : IMusicService, ICategoryParsingService
{
    private static readonly HashSet<string> OptionalTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".nfo",
        ".m3u",
        ".m3u8",
        ".cue"
    };

    private static readonly HashSet<string> IgnoredFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "thumbs.db"
    };

    private readonly HashSet<string> _musicExtensions = new(MusicCategory.MusicExtensions, StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _imageExtensions = new(ImageCategoryBase.ImageExtensions, StringComparer.OrdinalIgnoreCase);

    public bool IsMusicDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        var hasMusicFile = false;

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            if (IsIgnorable(file))
            {
                continue;
            }

            var extension = Path.GetExtension(file);
            if (_musicExtensions.Contains(extension))
            {
                hasMusicFile = true;
                continue;
            }

            if (_imageExtensions.Contains(extension) || OptionalTextExtensions.Contains(extension))
            {
                continue;
            }

            return false;
        }

        return hasMusicFile;
    }

    public bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory
    {
        return typeof(TCategory) == typeof(MusicCategory)
            && _musicExtensions.Contains(Path.GetExtension(filePath));
    }

    public bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory
    {
        return typeof(TCategory) == typeof(MusicCategory) && IsMusicDirectory(folderPath);
    }

    private static bool IsIgnorable(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (!string.IsNullOrWhiteSpace(fileName) && IgnoredFileNames.Contains(fileName))
        {
            return true;
        }

        try
        {
            var attributes = File.GetAttributes(filePath);
            if ((attributes & FileAttributes.Hidden) != 0 || (attributes & FileAttributes.System) != 0)
            {
                return true;
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return fileName?.StartsWith('.') == true;
    }
}
