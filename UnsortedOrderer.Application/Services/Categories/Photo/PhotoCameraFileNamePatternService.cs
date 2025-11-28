using System.Collections.Generic;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Services.Categories.Photo;

public sealed class PhotoCameraFileNamePatternService : CameraFileNamePatternService
{
    public PhotoCameraFileNamePatternService(IReadOnlyCollection<DeviceBrandPattern> patterns)
        : base(CameraMediaType.Photo, patterns)
    {
    }
}
