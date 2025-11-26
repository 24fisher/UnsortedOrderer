using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Services;

internal sealed class CategoryCache
{
    /// <summary>
    /// Provides cached extension lookups for all categories, keeping image-specific extension logic
    /// available while avoiding repeated scans across the full category list.
    /// </summary>
    public IReadOnlyDictionary<string, IFileCategory> CategoriesByExtension { get; }

    public HashSet<string> ImageCategoryExtensions { get; }

    public CategoryCache(IEnumerable<IFileCategory> categories)
    {
        var categoryArray = categories.ToArray();
        var imageCategories = categoryArray.OfType<ImageCategoryBase>().ToArray();
        var imageCategorySet = new HashSet<IFileCategory>(imageCategories.Cast<IFileCategory>());

        CategoriesByExtension = BuildCategoryExtensionLookup(categoryArray, imageCategorySet);
        ImageCategoryExtensions = BuildImageExtensionSet(imageCategories);
    }

    private static IReadOnlyDictionary<string, IFileCategory> BuildCategoryExtensionLookup(
        IEnumerable<IFileCategory> categories,
        IReadOnlySet<IFileCategory> imageCategorySet)
    {
        var lookup = new Dictionary<string, IFileCategory>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            if (imageCategorySet.Contains(category))
            {
                continue;
            }

            foreach (var extension in category.Extensions)
            {
                lookup[extension] = category;
            }
        }

        return lookup;
    }

    private static HashSet<string> BuildImageExtensionSet(IEnumerable<ImageCategoryBase> imageCategories)
    {
        return imageCategories
            .SelectMany(category => category.Extensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
