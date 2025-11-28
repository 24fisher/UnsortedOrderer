using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Services.Categories.Photo;

public sealed class DocumentImageParsingService : ICategoryParsingService
{
    private readonly HashSet<string> _documentImageKeywords;
    private readonly HashSet<string> _imageExtensions;

    public DocumentImageParsingService(AppSettings settings)
    {
        _documentImageKeywords = settings.DocumentImageKeywords
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Select(keyword => keyword.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _imageExtensions = new HashSet<string>(ImageCategoryBase.ImageExtensions, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory
    {
        if (typeof(TCategory) != typeof(DocumentsCategory))
        {
            return false;
        }

        if (_documentImageKeywords.Count == 0)
        {
            return false;
        }

        var extension = Path.GetExtension(filePath);
        if (!_imageExtensions.Contains(extension))
        {
            return false;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var lowerInvariantName = fileName.ToLowerInvariant();
        return _documentImageKeywords.Any(keyword => lowerInvariantName.Contains(keyword));
    }

    public bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory
    {
        return false;
    }
}
