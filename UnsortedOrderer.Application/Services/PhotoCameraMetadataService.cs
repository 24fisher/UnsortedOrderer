using System.Drawing;
using System.Text;
using UnsortedOrderer.Contracts.Services;

namespace UnsortedOrderer.Services;

public sealed class PhotoCameraMetadataService : IPhotoCameraMetadataService
{
    private const int CameraMakeId = 0x010F; // PropertyTagEquipMake
    private const int CameraModelId = 0x0110; // PropertyTagEquipModel

    public string? GetCameraFolder(string filePath)
    {
        try
        {
            using var image = Image.FromFile(filePath);
            var make = GetPropertyString(image, CameraMakeId);
            var model = GetPropertyString(image, CameraModelId);

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
        catch
        {
            return null;
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
