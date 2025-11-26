using UnsortedOrderer.Cache;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Services;

internal sealed class SpecialCategoriesHandler
{
    private readonly IReadOnlyCollection<ImageCategoryBase> _imageCategories;
    private readonly DocumentsCategory? _documentsCategory;
    private readonly CategoryCache _categoryCache;
    private readonly IReadOnlyCollection<string> _documentImageKeywords;

    public SpecialCategoriesHandler(
        IReadOnlyCollection<ImageCategoryBase> imageCategories,
        DocumentsCategory? documentsCategory,
        CategoryCache categoryCache,
        IReadOnlyCollection<string> documentImageKeywords)
    {
        _imageCategories = imageCategories;
        _documentsCategory = documentsCategory;
        _categoryCache = categoryCache;
        _documentImageKeywords = documentImageKeywords
            .Select(keyword => keyword.ToLowerInvariant())
            .ToArray();
    }

    public IFileCategory? TryGetSpecialCategory(string filePath, string extension)
    {
        if (_imageCategories.Count == 0 || _documentsCategory is null)
        {
            return null;
        }

        if (!_categoryCache.ImageCategoryExtensions.Contains(extension))
        {
            return null;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var lowerInvariantName = fileName.ToLowerInvariant();

        return _documentImageKeywords.Any(keyword => lowerInvariantName.Contains(keyword))
            ? _documentsCategory
            : null;
    }
}
