using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Linq;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
namespace UnsortedOrderer.Services;

public sealed class PhotoService : IPhotoService, IFileCategoryParsingService
{
    private readonly ICameraFileNamePatternService _cameraFileNamePatternService;
    private readonly IPhotoCameraMetadataService _photoCameraMetadataService;
    private readonly IMessengerPathService _messengerPathService;
    private const int SmallImageMaxDimension = 512;
    private const long SmallImageMaxBytes = 300 * 1024;
    private const int DateTakenId = 0x9003; // PropertyTagExifDTOrig
    private static readonly string[] ExifDateFormats =
    [
        "yyyy:MM:dd HH:mm:ss",
        "yyyy:MM:dd HH:mm:ssK",
        "yyyy:MM:dd HH:mm:sszzz"
    ];

    public PhotoService(
        IEnumerable<ICameraFileNamePatternService> cameraFileNamePatternServices,
        IPhotoCameraMetadataService photoCameraMetadataService,
        IMessengerPathService messengerPathService)
    {
        _cameraFileNamePatternService = cameraFileNamePatternServices
            .FirstOrDefault(service => service.MediaType == CameraMediaType.Photo)
            ?? throw new InvalidOperationException("Photo camera file name pattern service is not configured.");
        _photoCameraMetadataService = photoCameraMetadataService;
        _messengerPathService = messengerPathService;
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

    public bool IsFileOfCategory<TCategory>(string filePath)
        where TCategory : ICategory
    {
        return typeof(TCategory) == typeof(PhotosCategory) && IsPhoto(filePath);
    }

    public bool IsFolderOfCategory<TCategory>(string folderPath)
        where TCategory : ICategory
    {
        return false;
    }

    public string MovePhoto(
        string filePath,
        string destinationRoot,
        string photosFolderName,
        string? messengerFolderName = null)
    {
        var date = GetPhotoDate(filePath);
        var brandFolder = _photoCameraMetadataService.GetCameraFolder(filePath)
            ?? _cameraFileNamePatternService.GetBrandByFileName(Path.GetFileName(filePath));
        var photoDirectory = Path.Combine(
            destinationRoot,
            photosFolderName);

        var messengerFolder = messengerFolderName ?? _messengerPathService.GetMessengerFolder(filePath);
        if (!string.IsNullOrWhiteSpace(messengerFolder))
        {
            photoDirectory = Path.Combine(photoDirectory, messengerFolder);
        }

        photoDirectory = Path.Combine(
            photoDirectory,
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
                if (TryParseDateTaken(dateString, out var dateTaken))
                {
                    if (dateTaken.Year < 1980)
                    {
                        return File.GetCreationTime(filePath);
                    }

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

    private static bool TryParseDateTaken(string dateString, out DateTime dateTaken)
    {
        dateTaken = default;

        if (string.IsNullOrWhiteSpace(dateString))
        {
            return false;
        }

        var normalized = dateString.Trim('\0', ' ');

        if (DateTime.TryParseExact(
                normalized,
                ExifDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out dateTaken))
        {
            return true;
        }

        return DateTime.TryParse(normalized, out dateTaken);
    }
}
