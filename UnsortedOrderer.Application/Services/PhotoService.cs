using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
namespace UnsortedOrderer.Services;

public sealed class PhotoService : IPhotoService
{
    private readonly ICameraFileNamePatternService _cameraFileNamePatternService;
    private readonly IPhotoCameraMetadataService _photoCameraMetadataService;
    private const int SmallImageMaxDimension = 512;
    private const long SmallImageMaxBytes = 300 * 1024;
    private const int DateTakenId = 0x9003; // PropertyTagExifDTOrig

    public PhotoService(
        IEnumerable<ICameraFileNamePatternService> cameraFileNamePatternServices,
        IPhotoCameraMetadataService photoCameraMetadataService)
    {
        _cameraFileNamePatternService = cameraFileNamePatternServices
            .FirstOrDefault(service => service.MediaType == CameraMediaType.Photo)
            ?? throw new InvalidOperationException("Photo camera file name pattern service is not configured.");
        _photoCameraMetadataService = photoCameraMetadataService;
    }

    public bool IsPhoto(string filePath)
    {
        var extension = Path.GetExtension(filePath);

        if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            using var image = Image.FromFile(filePath);
            var maxDimension = Math.Max(image.Width, image.Height);

            if (maxDimension > SmallImageMaxDimension)
            {
                return true;
            }

            if (maxDimension <= SmallImageMaxDimension)
            {
                var fileSize = new FileInfo(filePath).Length;
                return fileSize > SmallImageMaxBytes;
            }
        }
        catch
        {
            // fall back to file heuristics
        }

        return new FileInfo(filePath).Length > SmallImageMaxBytes;
    }

    public string MovePhoto(
        string filePath,
        string destinationRoot,
        string photosFolderName)
    {
        var date = GetPhotoDate(filePath);
        var brandFolder = _photoCameraMetadataService.GetCameraFolder(filePath)
            ?? _cameraFileNamePatternService.GetBrandByFileName(Path.GetFileName(filePath));
        var photoDirectory = Path.Combine(
            destinationRoot,
            photosFolderName,
            date.Year.ToString(),
            date.Month.ToString("D2"));

        if (!string.IsNullOrWhiteSpace(brandFolder))
        {
            photoDirectory = Path.Combine(photoDirectory, brandFolder);
        }

        return FileUtilities.MoveFile(filePath, photoDirectory);
    }

    private static DateTime GetPhotoDate(string filePath)
    {
        try
        {
            using var image = Image.FromFile(filePath);
            var propertyItem = image.PropertyItems.FirstOrDefault(p => p.Id == DateTakenId);
            if (propertyItem is not null)
            {
                var dateString = System.Text.Encoding.ASCII.GetString(propertyItem.Value).Trim('\0');
                if (DateTime.TryParse(dateString, out var dateTaken))
                {
                    return dateTaken;
                }
            }
        }
        catch
        {
            // fall back to file timestamps
        }

        return File.GetCreationTime(filePath);
    }
}
