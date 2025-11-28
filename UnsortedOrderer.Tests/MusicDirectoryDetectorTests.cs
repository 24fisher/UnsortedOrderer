using System;
using System.IO;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class MusicDirectoryDetectorTests
{
    [Fact]
    public void DirectoryWithMusicArtworkAndHiddenFiles_IsDetected()
    {
        using var directory = new TempDirectory();
        var detector = new MusicDirectoryDetector();

        File.WriteAllText(Path.Combine(directory.Path, "song.mp3"), "");
        File.WriteAllText(Path.Combine(directory.Path, "cover.jpg"), "");
        File.WriteAllText(Path.Combine(directory.Path, "tracklist.txt"), "");
        File.WriteAllText(Path.Combine(directory.Path, "playlist.m3u"), "");
        File.WriteAllText(Path.Combine(directory.Path, ".DS_Store"), "");
        File.WriteAllText(Path.Combine(directory.Path, "thumbs.db"), "");

        Assert.True(detector.IsMusicDirectory(directory.Path));
    }

    [Fact]
    public void NestedDirectoriesWithMusic_AreDetected()
    {
        using var directory = new TempDirectory();
        var detector = new MusicDirectoryDetector();

        var disc1 = Directory.CreateDirectory(Path.Combine(directory.Path, "Disc 1"));
        File.WriteAllText(Path.Combine(disc1.FullName, "intro.flac"), "");
        File.WriteAllText(Path.Combine(disc1.FullName, "artwork.png"), "");

        Assert.True(detector.IsMusicDirectory(directory.Path));
    }

    [Fact]
    public void DirectoryWithNonMusicFile_IsNotDetected()
    {
        using var directory = new TempDirectory();
        var detector = new MusicDirectoryDetector();

        File.WriteAllText(Path.Combine(directory.Path, "song.m4a"), "");
        File.WriteAllText(Path.Combine(directory.Path, "notes.pdf"), "");

        Assert.False(detector.IsMusicDirectory(directory.Path));
    }

    [Fact]
    public void DirectoryWithoutMusicFiles_IsNotDetected()
    {
        using var directory = new TempDirectory();
        var detector = new MusicDirectoryDetector();

        File.WriteAllText(Path.Combine(directory.Path, "cover.jpeg"), "");
        File.WriteAllText(Path.Combine(directory.Path, "playlist.txt"), "");

        Assert.False(detector.IsMusicDirectory(directory.Path));
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
            }
        }
    }
}
