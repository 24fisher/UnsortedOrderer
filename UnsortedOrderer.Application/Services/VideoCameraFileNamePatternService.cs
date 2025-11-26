using System.Collections.Generic;
using UnsortedOrderer.Models;

namespace UnsortedOrderer.Services;

public sealed class VideoCameraFileNamePatternService : CameraFileNamePatternService
{
    public VideoCameraFileNamePatternService(IReadOnlyCollection<DeviceBrandPattern> patterns)
        : base(CameraMediaType.Video, patterns)
    {
    }
}
