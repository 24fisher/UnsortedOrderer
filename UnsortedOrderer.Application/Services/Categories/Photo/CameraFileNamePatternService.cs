using System.Collections.Generic;
using UnsortedOrderer.Application.Contracts.Services.Categories.Photo;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Services.Categories.Photo;

public abstract class CameraFileNamePatternService : ICameraFileNamePatternService
{
    private readonly IReadOnlyCollection<DeviceBrandPattern> _patterns;

    protected CameraFileNamePatternService(CameraMediaType mediaType, IReadOnlyCollection<DeviceBrandPattern> patterns)
    {
        MediaType = mediaType;
        _patterns = patterns ?? Array.Empty<DeviceBrandPattern>();
    }

    public CameraMediaType MediaType { get; }

    public string? GetBrandByFileName(string? fileName)
    {
        return DeviceBrandMatcher.GetBrandByFileName(fileName, _patterns);
    }
}
