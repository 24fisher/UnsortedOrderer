using System.Collections.Generic;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

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
