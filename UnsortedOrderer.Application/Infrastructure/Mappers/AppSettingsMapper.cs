using System.Text.RegularExpressions;
using System.Linq;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Infrastructure.Mappers;

public static class AppSettingsMapper
{
    public static AppSettings Map(AppSettingsDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new AppSettings(
            Require(dto.SourceDirectory, nameof(dto.SourceDirectory)),
            Require(dto.DestinationRoot, nameof(dto.DestinationRoot)),
            Require(dto.SoftFolderName, nameof(dto.SoftFolderName)),
            Require(dto.ArchiveFolderName, nameof(dto.ArchiveFolderName)),
            Require(dto.ImagesFolderName, nameof(dto.ImagesFolderName)),
            Require(dto.PhotosFolderName, nameof(dto.PhotosFolderName)),
            Require(dto.MusicFolderName, nameof(dto.MusicFolderName)),
            Require(dto.MusicalInstrumentsFolderName, nameof(dto.MusicalInstrumentsFolderName)),
            Require(dto.EBooksFolderName, nameof(dto.EBooksFolderName)),
            Require(dto.RepositoriesFolderName, nameof(dto.RepositoriesFolderName)),
            Require(dto.DriversFolderName, nameof(dto.DriversFolderName)),
            Require(dto.FirmwareFolderName, nameof(dto.FirmwareFolderName)),
            Require(dto.MetadataFolderName, nameof(dto.MetadataFolderName)),
            Require(dto.UnknownFolderName, nameof(dto.UnknownFolderName)),
            dto.DeletedExtensions ?? Array.Empty<string>(),
            dto.DocumentImageKeywords ?? Array.Empty<string>(),
            MapCameraFileNamePatterns(dto.CameraFileNamePatterns),
            dto.SoftwareArchiveKeywords ?? Array.Empty<string>());
    }

    private static IReadOnlyCollection<DeviceBrandPattern> MapCameraFileNamePatterns(
        IEnumerable<DeviceBrandPatternDto>? patternDtos)
    {
        if (patternDtos is null)
        {
            return Array.Empty<DeviceBrandPattern>();
        }

        return patternDtos
            .Select(patternDto =>
            {
                if (patternDto is null)
                {
                    throw new InvalidOperationException("Camera file name pattern entry cannot be null.");
                }

                var brand = Require(patternDto.Brand, nameof(patternDto.Brand));
                var pattern = Require(patternDto.Pattern, nameof(patternDto.Pattern));

                return new DeviceBrandPattern(
                    brand,
                    new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            })
            .ToArray();
    }

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{name}' is missing or empty.");
        }

        return value;
    }
}
