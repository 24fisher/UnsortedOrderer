using System;
using System.IO;
using UnsortedOrderer.Application.Contracts.Services.Categories;
using UnsortedOrderer.Application.Contracts.Services.Categories.Photo;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using Xunit;

namespace UnsortedOrderer.Tests;

public class VideosCategoryTests
{
    [Fact]
    public void GetFileDestination_places_video_by_month_and_messenger()
    {
        var videoDateService = new StubVideoDateService(new DateTime(2025, 4, 27));
        var messengerService = new StubMessengerPathService("_Telegram");
        var cameraPatternService = new StubCameraPatternService(CameraMediaType.Video, "Honor");
        var category = new VideosCategory(
            new[] { cameraPatternService },
            videoDateService,
            messengerService);

        var destination = category.GetFileDestination("/dest", "/source/telegram/SL_MO_VID_20250427_145754.mp4");

        Assert.Equal(
            Path.Combine("/dest", "Videos", "_Telegram", "2025", "04", "Honor"),
            destination);
    }

    private sealed class StubVideoDateService : IVideoDateService
    {
        public StubVideoDateService(DateTime date)
        {
            _date = date;
        }

        private readonly DateTime _date;

        public DateTime GetVideoDate(string filePath) => _date;
    }

    private sealed class StubMessengerPathService : IMessengerPathService
    {
        public StubMessengerPathService(string? folder)
        {
            _folder = folder;
        }

        private readonly string? _folder;

        public string? GetMessengerFolder(string filePath) => _folder;
    }

    private sealed class StubCameraPatternService : ICameraFileNamePatternService
    {
        public StubCameraPatternService(CameraMediaType mediaType, string? brand)
        {
            MediaType = mediaType;
            _brand = brand;
        }

        private readonly string? _brand;

        public CameraMediaType MediaType { get; }

        public string? GetBrandByFileName(string? fileName)
        {
            return _brand;
        }
    }
}
