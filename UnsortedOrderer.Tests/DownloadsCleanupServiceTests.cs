using System;
using System.IO;
using System.Reflection;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class DownloadsCleanupServiceTests
{
    [Fact]
    public void EnsureUniquePath_AppendsCounter_WhenDestinationExists()
    {
        var ensureUniquePathMethod = typeof(DownloadsCleanupService)
            .GetMethod("EnsureUniquePath", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("EnsureUniquePath method not found.");

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var existingFile = Path.Combine(tempDirectory, "file.txt");
            File.WriteAllText(existingFile, string.Empty);

            var result = (string)ensureUniquePathMethod.Invoke(null, new object[] { existingFile })!;

            Assert.Equal(Path.Combine(tempDirectory, "file (1).txt"), result);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void MoveEntry_RelocatesDirectory()
    {
        var moveEntryMethod = typeof(DownloadsCleanupService)
            .GetMethod("MoveEntry", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MoveEntry method not found.");

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var sourceDirectory = Path.Combine(tempDirectory, "source");
        Directory.CreateDirectory(sourceDirectory);

        var destinationDirectory = Path.Combine(tempDirectory, "destination");

        try
        {
            moveEntryMethod.Invoke(null, new object[] { new DirectoryInfo(sourceDirectory), destinationDirectory });

            Assert.True(Directory.Exists(destinationDirectory));
            Assert.False(Directory.Exists(sourceDirectory));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
