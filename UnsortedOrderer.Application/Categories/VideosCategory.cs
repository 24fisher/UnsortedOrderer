using System.Collections.Generic;
using System.Linq;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Categories;

public sealed class VideosCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".mpeg"
    ];

    private readonly ICameraFileNamePatternService _cameraFileNamePatternService;
    private readonly IVideoDateService _videoDateService;
    private readonly IMessengerPathService _messengerPathService;

    public VideosCategory(
        IEnumerable<ICameraFileNamePatternService> cameraFileNamePatternServices,
        IVideoDateService videoDateService,
        IMessengerPathService messengerPathService)
        : base("Videos", "Videos", VideoExtensions)
    {
        _cameraFileNamePatternService = cameraFileNamePatternServices
            .FirstOrDefault(service => service.MediaType == CameraMediaType.Video)
            ?? throw new InvalidOperationException("Video camera file name pattern service is not configured.");
        _videoDateService = videoDateService;
        _messengerPathService = messengerPathService;
    }

    public bool IsNonSplittableDirectory(string path)
    {
        return false;
    }

    public string GetDirectoryDestination(string destinationRoot, string directoryPath)
    {
        return Path.Combine(destinationRoot, FolderName);
    }

    public string GetFileDestination(string destinationRoot, string filePath)
    {
        var date = _videoDateService.GetVideoDate(filePath);
        var cameraFolder = GetCameraSubfolder(Path.GetFileName(filePath));
        var messengerFolder = _messengerPathService.GetMessengerFolder(filePath);
        var baseDirectory = Path.Combine(destinationRoot, FolderName);

        if (!string.IsNullOrWhiteSpace(messengerFolder))
        {
            baseDirectory = Path.Combine(baseDirectory, messengerFolder);
        }

        baseDirectory = Path.Combine(baseDirectory, date.Year.ToString(), date.Month.ToString("D2"));

        return cameraFolder is null
            ? baseDirectory
            : Path.Combine(baseDirectory, cameraFolder);
    }

    private string? GetCameraSubfolder(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return _cameraFileNamePatternService.GetBrandByFileName(fileName);
    }
}
