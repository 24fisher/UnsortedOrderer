using System.Collections.Generic;
using UnsortedOrderer.Application.Services.Categories.Photo;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Application.Services.Categories.Video;

public sealed class VideoCameraFileNamePatternService : CameraFileNamePatternService
{
    public VideoCameraFileNamePatternService(IReadOnlyCollection<DeviceBrandPattern> patterns)
        : base(CameraMediaType.Video, patterns)
    {
    }
}
