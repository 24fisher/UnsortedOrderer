using System;
using System.IO;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class VideoDateServiceTests
{
    [Fact]
    public void GetVideoDate_falls_back_to_creation_time_when_metadata_missing()
    {
        using var tempDirectory = new TempDirectory();
        var filePath = Path.Combine(tempDirectory.Path, "video.mp4");
        File.WriteAllText(filePath, "dummy");

        var creationDate = new DateTime(2023, 12, 1, 10, 30, 0);
        File.SetCreationTime(filePath, creationDate);
        File.SetLastWriteTime(filePath, creationDate);

        var service = new VideoDateService();
        var result = service.GetVideoDate(filePath);

        Assert.Equal(creationDate, result);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch
            {
                // ignore cleanup errors in tests
            }
        }
    }
}
