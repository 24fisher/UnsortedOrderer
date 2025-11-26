using System.Collections.Generic;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class PhotoCameraFileNamePatternService : CameraFileNamePatternService
{
    public PhotoCameraFileNamePatternService(IReadOnlyCollection<DeviceBrandPattern> patterns)
        : base(CameraMediaType.Photo, patterns)
    {
    }
}
