using System.Collections.Generic;
using System.IO;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Application.Services.Categories.Video;

public sealed class VideoService : ICategoryParsingService
{
    private readonly HashSet<string> _videoExtensions;

    public VideoService()
    {
        _videoExtensions = new HashSet<string>(VideosCategory.SupportedExtensions, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory
    {
        if (typeof(TCategory) != typeof(VideosCategory))
        {
            return false;
        }

        var extension = Path.GetExtension(filePath);
        return _videoExtensions.Contains(extension);
    }

    public bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory
    {
        return false;
    }
}
