using System.Collections.Generic;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class CategoryExtensionsTests
{
    [Fact]
    public void Categories_should_not_have_overlapping_extensions_except_images()
    {
        var categories = CreateCategories();

        var overlaps = FindExtensionOverlaps(categories)
            .Where(kvp => !kvp.Value.All(category => category is ImageCategoryBase))
            .ToArray();

        var overlapDetails = overlaps
            .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Select(c => c.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase))}")
            .ToArray();

        Assert.True(
            overlaps.Length == 0,
            $"Category extensions overlap: {string.Join("; ", overlapDetails)}");
    }

    private static IReadOnlyCollection<ICategory> CreateCategories()
    {
        return new ICategory[]
        {
            new PhotosCategory("Photos"),
            new ImagesCategory("Images"),
            new MusicCategory("Music", new StubMusicDirectoryDetector()),
            new MusicalInstrumentsCategory("MusicalInstruments"),
            new EBooksCategory("EBooks"),
            new DocumentsCategory(),
            new WebCategory("Web"),
            new GraphicsCategory("Graphics"),
            new VideosCategory(
                CreateVideoCameraFileNamePatternServices(),
                new StubVideoDateService(),
                new StubMessengerPathService()),
            new ThreeDModelsCategory(),
            new ArchivesCategory("Archives"),
            new CertificatesCategory(),
            new FirmwareCategory("Firmware"),
            new MetadataCategory("Metadata"),
            new DriversCategory("Drivers"),
            new RepositoriesCategory(
                "Repositories",
                new RepositoryDetector(RepositoriesCategory.CodeExtensions, 3)),
            new SoftCategory("Soft", new SoftwareDistributivesDetector()),
            new UnknownCategory("Unknown"),
        };
    }

    private static IEnumerable<ICameraFileNamePatternService> CreateVideoCameraFileNamePatternServices()
    {
        return new ICameraFileNamePatternService[]
        {
            new VideoCameraFileNamePatternService(Array.Empty<DeviceBrandPattern>())
        };
    }

    private sealed class StubVideoDateService : IVideoDateService
    {
        public DateTime GetVideoDate(string filePath)
        {
            return DateTime.UtcNow;
        }
    }

    private sealed class StubMessengerPathService : IMessengerPathService
    {
        public string? GetMessengerFolder(string filePath) => null;
    }

    private sealed class StubMusicDirectoryDetector : IMusicDirectoryDetector
    {
        public bool IsMusicDirectory(string path) => false;
    }

    private static IReadOnlyDictionary<string, IReadOnlyCollection<ICategory>> FindExtensionOverlaps(
        IEnumerable<ICategory> categories)
    {
        var lookup = new Dictionary<string, HashSet<ICategory>>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            foreach (var extension in category.Extensions)
            {
                if (!lookup.TryGetValue(extension, out var categorySet))
                {
                    categorySet = new HashSet<ICategory>();
                    lookup[extension] = categorySet;
                }

                categorySet.Add(category);
            }
        }

        return lookup
            .Where(kvp => kvp.Value.Count > 1)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyCollection<ICategory>)kvp.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }
}
