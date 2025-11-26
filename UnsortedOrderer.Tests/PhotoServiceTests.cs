using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class PhotoServiceTests
{
    [Fact]
    public void MovePhoto_uses_metadata_folder_when_available()
    {
        using var tempDirectory = new TempDirectory();
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        var photoPath = CreateImage(tempDirectory.Path, new DateTime(2020, 1, 15));

        var metadataService = new StubPhotoCameraMetadataService("Canon EOS 80D");
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, "PatternBrand");
        var photoService = new PhotoService(new[] { patternService }, metadataService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.True(File.Exists(destination));
        Assert.Contains(Path.Combine(destinationRoot, "Photos", "2020", "01", "Canon EOS 80D"), destination);
        Assert.DoesNotContain("PatternBrand", destination);
    }

    [Fact]
    public void MovePhoto_falls_back_to_pattern_when_metadata_is_missing()
    {
        using var tempDirectory = new TempDirectory();
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        var photoPath = CreateImage(tempDirectory.Path, new DateTime(2021, 6, 3));

        var metadataService = new StubPhotoCameraMetadataService(null);
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, "PatternBrand");
        var photoService = new PhotoService(new[] { patternService }, metadataService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.True(File.Exists(destination));
        Assert.Contains(Path.Combine(destinationRoot, "Photos", "2021", "06", "PatternBrand"), destination);
    }

    [Fact]
    public void Metadata_service_reads_make_and_model_from_exif()
    {
        using var tempDirectory = new TempDirectory();
        var photoPath = CreateImageWithExif(tempDirectory.Path, "Canon", "Canon EOS 80D");
        var service = new PhotoCameraMetadataService();

        var folder = service.GetCameraFolder(photoPath);

        Assert.Equal("Canon EOS 80D", folder);
    }

    [Fact]
    public void Metadata_service_sanitizes_invalid_characters_and_removes_duplicate_make_prefix()
    {
        using var tempDirectory = new TempDirectory();
        var photoPath = CreateImageWithExif(tempDirectory.Path, "Sony", "Sony/ILCE-7M3");
        var service = new PhotoCameraMetadataService();

        var folder = service.GetCameraFolder(photoPath);

        Assert.Equal("Sony ILCE-7M3", folder);
    }

    [Theory]
    [InlineData(".png", ImageFormat.Png)]
    [InlineData(".gif", ImageFormat.Gif)]
    public void IsPhoto_returns_false_for_large_png_and_gif(string extension, ImageFormat format)
    {
        using var tempDirectory = new TempDirectory();
        var metadataService = new StubPhotoCameraMetadataService(null);
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, null);
        var photoService = new PhotoService(new[] { patternService }, metadataService);
        var imagePath = CreateLargeImage(tempDirectory.Path, extension, format);

        var result = photoService.IsPhoto(imagePath);

        Assert.False(result);
    }

    private static string CreateImage(string directory, DateTime creationTime)
    {
        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.jpg");
        using var bitmap = new Bitmap(4, 4);
        bitmap.Save(path, ImageFormat.Jpeg);

        File.SetCreationTime(path, creationTime);
        File.SetLastWriteTime(path, creationTime);

        return path;
    }

    private static string CreateImageWithExif(string directory, string make, string model)
    {
        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.jpg");
        using var bitmap = new Bitmap(4, 4);
        SetAsciiProperty(bitmap, 0x010F, make); // Make
        SetAsciiProperty(bitmap, 0x0110, model); // Model
        bitmap.Save(path, ImageFormat.Jpeg);

        return path;
    }

    private static string CreateLargeImage(string directory, string extension, ImageFormat format)
    {
        var path = Path.Combine(directory, $"{Guid.NewGuid():N}{extension}");
        using var bitmap = new Bitmap(1024, 1024);
        bitmap.Save(path, format);

        return path;
    }

    private static void SetAsciiProperty(Image image, int id, string value)
    {
        var propertyItem = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        propertyItem.Id = id;
        propertyItem.Type = 2; // ASCII
        propertyItem.Value = System.Text.Encoding.ASCII.GetBytes(value + "\0");
        propertyItem.Len = propertyItem.Value.Length;
        image.SetPropertyItem(propertyItem);
    }

    private sealed class StubCameraPatternService : ICameraFileNamePatternService
    {
        public StubCameraPatternService(CameraMediaType mediaType, string? brand)
        {
            MediaType = mediaType;
            _brand = brand;
        }

        private readonly string? _brand;

        public CameraMediaType MediaType { get; }

        public string? GetBrandByFileName(string? fileName)
        {
            return _brand;
        }
    }

    private sealed class StubPhotoCameraMetadataService : IPhotoCameraMetadataService
    {
        public StubPhotoCameraMetadataService(string? folder)
        {
            _folder = folder;
        }

        private readonly string? _folder;

        public string? GetCameraFolder(string filePath)
        {
            return _folder;
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch
            {
                // ignore cleanup errors in tests
            }
        }
    }
}
