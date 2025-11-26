using UnsortedOrderer.Models;

namespace UnsortedOrderer.Contracts.Services;

public interface ICameraFileNamePatternService
{
    CameraMediaType MediaType { get; }

    string? GetBrandByFileName(string? fileName);
}
