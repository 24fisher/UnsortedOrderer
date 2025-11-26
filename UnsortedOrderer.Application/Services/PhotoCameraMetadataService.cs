using System.Drawing;
using System.Linq;
using System.Text;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class PhotoCameraMetadataService : IPhotoCameraMetadataService
{
    private const int CameraMakeId = 0x010F; // PropertyTagEquipMake
    private const int CameraModelId = 0x0110; // PropertyTagEquipModel

    public string? GetCameraFolder(string filePath)
    {
        var (make, model) = TryGetCameraMetadata(filePath);

        if (string.IsNullOrWhiteSpace(make))
        {
            return null;
        }

        var normalizedMake = NormalizePathSegment(make);
        var normalizedModel = NormalizePathSegment(model);

        if (string.IsNullOrWhiteSpace(normalizedMake))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(normalizedModel))
        {
            return normalizedMake;
        }

        var modelWithoutMakePrefix = normalizedModel.StartsWith(normalizedMake, StringComparison.OrdinalIgnoreCase)
            ? normalizedModel[normalizedMake.Length..].TrimStart(' ', '-', '_')
            : normalizedModel;

        if (string.IsNullOrWhiteSpace(modelWithoutMakePrefix))
        {
            return normalizedMake;
        }

        return $"{normalizedMake} {modelWithoutMakePrefix}";
    }

    private static (string? Make, string? Model) TryGetCameraMetadata(string filePath)
    {
        var (make, model) = TryGetMetadataExtractorValues(filePath);

        if (!string.IsNullOrWhiteSpace(make) || !string.IsNullOrWhiteSpace(model))
        {
            return (make, model);
        }

        return TryGetSystemDrawingValues(filePath);
    }

    private static (string? Make, string? Model) TryGetMetadataExtractorValues(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            var exifDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (exifDirectory is null)
            {
                return (null, null);
            }

            var make = exifDirectory.GetDescription(ExifDirectoryBase.TagMake);
            var model = exifDirectory.GetDescription(ExifDirectoryBase.TagModel);

            return (make, model);
        }
        catch
        {
            return (null, null);
        }
    }

    private static (string? Make, string? Model) TryGetSystemDrawingValues(string filePath)
    {
        try
        {
            using var image = Image.FromFile(filePath);
            var make = GetPropertyString(image, CameraMakeId);
            var model = GetPropertyString(image, CameraModelId);

            return (make, model);
        }
        catch
        {
            return (null, null);
        }
    }

    private static string? GetPropertyString(Image image, int propertyId)
    {
        var propertyItem = image.PropertyItems.FirstOrDefault(item => item.Id == propertyId);
        if (propertyItem is null || propertyItem.Value is null)
        {
            return null;
        }

        return Encoding.ASCII.GetString(propertyItem.Value).Trim('\0', ' ');
    }

    private static string? NormalizePathSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedChars = trimmed
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray();

        return new string(sanitizedChars).Trim();
    }
}
