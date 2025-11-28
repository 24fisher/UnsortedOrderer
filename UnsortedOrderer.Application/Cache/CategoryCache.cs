using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Cache;

internal sealed class CategoryCache
{
    /// <summary>
    /// Provides cached extension lookups for all categories, keeping image-specific extension logic
    /// available while avoiding repeated scans across the full category list.
    /// </summary>
    public IReadOnlyDictionary<string, ICategory> CategoriesByExtension { get; }

    public HashSet<string> ImageCategoryExtensions { get; }

    public CategoryCache(IEnumerable<ICategory> categories)
    {
        var categoryArray = categories.ToArray();
        var imageCategories = categoryArray.OfType<ImageCategoryBase>().ToArray();
        var imageCategorySet = new HashSet<ICategory>(imageCategories.Cast<ICategory>());

        CategoriesByExtension = BuildCategoryExtensionLookup(categoryArray, imageCategorySet);
        ImageCategoryExtensions = BuildImageExtensionSet(imageCategories);
    }

    private static IReadOnlyDictionary<string, ICategory> BuildCategoryExtensionLookup(
        IEnumerable<ICategory> categories,
        IReadOnlySet<ICategory> imageCategorySet)
    {
        var lookup = new Dictionary<string, ICategory>(StringComparer.OrdinalIgnoreCase);

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
