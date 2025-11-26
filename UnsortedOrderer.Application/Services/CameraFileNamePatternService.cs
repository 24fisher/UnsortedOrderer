using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class CameraFileNamePatternService : ICameraFileNamePatternService
{
    private readonly IReadOnlyCollection<DeviceBrandPattern> _patterns;

    public CameraFileNamePatternService(IReadOnlyCollection<DeviceBrandPattern> patterns)
    {
        _patterns = patterns ?? Array.Empty<DeviceBrandPattern>();
    }

    public string? GetBrandByFileName(string? fileName)
    {
        return DeviceBrandMatcher.GetBrandByFileName(fileName, _patterns);
    }
}
