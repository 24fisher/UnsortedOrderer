using System.Text.RegularExpressions;
using UnsortedOrderer.Contracts.Categories;

namespace UnsortedOrderer.Categories;

public sealed class VideosCategory : FileCategory, INonSplittableDirectoryCategory
{
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".mpeg"
    ];

    private static readonly (string Brand, Regex Pattern)[] CameraPatterns =
    [
        ("DJI Action", new Regex(@"^DJI[-_]?\d+(?:_\d+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("GoPro", new Regex(@"^(?:GOPR\d{4}|G[HPXY]\d{6})", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Sony", new Regex(@"^(?:C\d{4}|DSC\d{5,}|MAH\d{5,})", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Canon", new Regex(@"^MVI_\d{4,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Nikon", new Regex(@"^DSCN\d{4,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Ricoh", new Regex(@"^R\d{7}(?:_[0-9A-Za-z]+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("iPhone", new Regex(@"^(?:IMG|VID|RPReplay_Final)_?\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Samsung", new Regex(@"^20\d{6}_\d{6}(?:_\d+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Google Pixel", new Regex(@"^PXL_\d{8}_\d{6,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Xiaomi", new Regex(@"^VID_\d{8}_\d{6}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Realme", new Regex(@"^VID\d{8}_\d{6}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Nokia", new Regex(@"^(?:NOKIA|WP)_\d{8}_\d{6}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Huawei", new Regex(@"^(?:IMG|VID)_\d{8}_\d{6}_HDR", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("OnePlus", new Regex(@"^VID_\d{8}_\d{6}_\d{3}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Motorola", new Regex(@"^VID_\d{8}_\d{6}_MP", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Oppo", new Regex(@"^(?:OPP?O|CPH)\d{4,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Vivo", new Regex(@"^VID_\d{8}_\d{6}VIV", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("LG", new Regex(@"^\d{8}_\d{6}_LG", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Asus", new Regex(@"^VID_\d{8}_\d{6}A", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("ZTE", new Regex(@"^(?:ZTE|NUBIA)_\d{8}_\d{6}", RegexOptions.IgnoreCase | RegexOptions.Compiled))
    ];

    public VideosCategory()
        : base("Videos", "Videos", VideoExtensions)
    {
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
        var year = GetVideoYear(filePath);
        var cameraFolder = GetCameraSubfolder(Path.GetFileName(filePath));
        var baseDirectory = Path.Combine(destinationRoot, FolderName, year.ToString());

        return cameraFolder is null
            ? baseDirectory
            : Path.Combine(baseDirectory, cameraFolder);
    }

    private static string? GetCameraSubfolder(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        foreach (var (brand, pattern) in CameraPatterns)
        {
            if (pattern.IsMatch(fileName))
            {
                return brand;
            }
        }

        return null;
    }

    private static int GetVideoYear(string filePath)
    {
        try
        {
            var creationTime = File.GetCreationTime(filePath);
            if (creationTime.Year > 1)
            {
                return creationTime.Year;
            }

            var lastWriteTime = File.GetLastWriteTime(filePath);
            if (lastWriteTime.Year > 1)
            {
                return lastWriteTime.Year;
            }
        }
        catch
        {
            return DateTime.Now.Year;
        }

        return DateTime.Now.Year;
    }
}
