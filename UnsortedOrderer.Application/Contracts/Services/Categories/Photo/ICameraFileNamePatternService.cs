using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Contracts.Services.Categories.Photo;

public interface ICameraFileNamePatternService
{
    CameraMediaType MediaType { get; }

    string? GetBrandByFileName(string? fileName);
}
