using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using UnsortedOrderer.Application.Contracts.Services.Categories.Photo;
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
        var messengerService = new StubMessengerPathService(null);
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);

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
        var messengerService = new StubMessengerPathService(null);
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.True(File.Exists(destination));
        Assert.Contains(Path.Combine(destinationRoot, "Photos", "2021", "06", "PatternBrand"), destination);
    }

    [Fact]
    public void MovePhoto_prefers_exif_date_taken_over_creation_time()
    {
        using var tempDirectory = new TempDirectory();
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        var exifDate = new DateTime(2020, 2, 3, 4, 5, 6);
        var creationDate = new DateTime(2024, 1, 1);
        var photoPath = CreateImageWithExifDate(tempDirectory.Path, exifDate, creationDate);

        var metadataService = new StubPhotoCameraMetadataService(null);
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, null);
        var messengerService = new StubMessengerPathService(null);
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.Contains(Path.Combine(destinationRoot, "Photos", "2020", "02"), destination);
        Assert.DoesNotContain(Path.Combine(destinationRoot, "Photos", "2024", "01"), destination);
    }

    [Fact]
    public void MovePhoto_uses_creation_date_when_exif_before_1980()
    {
        using var tempDirectory = new TempDirectory();
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        var exifDate = new DateTime(1970, 2, 3, 4, 5, 6);
        var creationDate = new DateTime(2024, 1, 1);
        var photoPath = CreateImageWithExifDate(tempDirectory.Path, exifDate, creationDate);

        var metadataService = new StubPhotoCameraMetadataService(null);
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, null);
        var messengerService = new StubMessengerPathService(null);
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.Contains(Path.Combine(destinationRoot, "Photos", "2024", "01"), destination);
        Assert.DoesNotContain(Path.Combine(destinationRoot, "Photos", "1970", "02"), destination);
    }

    [Fact]
    public void MovePhoto_places_telegram_photos_under_messenger_folder()
    {
        using var tempDirectory = new TempDirectory();
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        var creationDate = new DateTime(2022, 7, 8);
        var photoPath = CreateImage(tempDirectory.Path, creationDate);

        var metadataService = new StubPhotoCameraMetadataService(null);
        var patternService = new StubCameraPatternService(CameraMediaType.Photo, null);
        var messengerService = new StubMessengerPathService("_Telegram");
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);

        var destination = photoService.MovePhoto(photoPath, destinationRoot, "Photos");

        Assert.Contains(Path.Combine(destinationRoot, "Photos", "_Telegram", "2022", "07"), destination);
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
        var messengerService = new StubMessengerPathService(null);
        var photoService = new PhotoService(new[] { patternService }, metadataService, messengerService);
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

    private static string CreateImageWithExifDate(string directory, DateTime dateTaken, DateTime creationDate)
    {
        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.jpg");
        using var bitmap = new Bitmap(4, 4);
        SetAsciiProperty(bitmap, 0x9003, dateTaken.ToString("yyyy:MM:dd HH:mm:ss")); // DateTaken
        bitmap.Save(path, ImageFormat.Jpeg);

        File.SetCreationTime(path, creationDate);
        File.SetLastWriteTime(path, creationDate);

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

    private sealed class StubMessengerPathService : IMessengerPathService
    {
        public StubMessengerPathService(string? folder)
        {
            _folder = folder;
        }

        private readonly string? _folder;

        public string? GetMessengerFolder(string filePath)
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
