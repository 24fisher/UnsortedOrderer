using System;
using System.IO;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class MessengerPathServiceTests
{
    [Theory]
    [InlineData("Telegram", "_Telegram")]
    [InlineData("my_watsapp_media", "_Watsapp")]
    [InlineData("WhatsApp", "_Watsapp")]
    public void GetMessengerFolder_detects_messenger_in_path(string folderName, string expected)
    {
        using var tempDirectory = new TempDirectory();
        var messengerDirectory = Path.Combine(tempDirectory.Path, folderName, "Album");
        Directory.CreateDirectory(messengerDirectory);

        var filePath = Path.Combine(messengerDirectory, "image.jpg");
        File.WriteAllText(filePath, "test");

        var service = new MessengerPathService();
        var result = service.GetMessengerFolder(filePath);

        Assert.Equal(expected, result);
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
