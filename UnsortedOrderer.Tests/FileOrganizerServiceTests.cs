using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public class FileOrganizerServiceTests
{
    [Fact]
    public void Images_are_moved_into_images_subfolder()
    {
        using var tempDirectory = new TempDirectory();

        var sourceDirectory = Path.Combine(tempDirectory.Path, "Source");
        var destinationRoot = Path.Combine(tempDirectory.Path, "Dest");
        Directory.CreateDirectory(sourceDirectory);

        var settings = new AppSettings(
            sourceDirectory,
            destinationRoot,
            softFolderName: "Soft",
            archiveFolderName: "Archives",
            imagesFolderName: "Images",
            photosFolderName: "Photos",
            musicFolderName: "Music",
            musicalInstrumentsFolderName: "Instruments",
            eBooksFolderName: "EBooks",
            repositoriesFolderName: "Repositories",
            driversFolderName: "Drivers",
            firmwareFolderName: "Firmware",
            metadataFolderName: "Metadata",
            unknownFolderName: "_Unknown",
            deletedExtensions: Array.Empty<string>(),
            documentImageKeywords: Array.Empty<string>(),
            cameraFileNamePatterns: Array.Empty<DeviceBrandPattern>(),
            softwareArchiveKeywords: Array.Empty<string>());

        var imagePath = Path.Combine(sourceDirectory, "icon.png");
        File.WriteAllBytes(imagePath, new byte[] { 1, 2, 3 });

        var categories = new IFileCategory[]
        {
            new PhotosCategory(settings.PhotosFolderName),
            new ImagesCategory(settings.ImagesFolderName),
            new UnknownCategory(settings.UnknownFolderName)
        };

        var archiveService = new StubArchiveService();
        var photoService = new StubPhotoService(isPhoto: false);
        var statisticsService = new RecordingStatisticsService();
        var messageWriter = new StubMessageWriter();

        var organizer = new FileOrganizerService(
            settings,
            archiveService,
            photoService,
            categories,
            statisticsService,
            messageWriter);

        InvokeProcessFile(organizer, imagePath);

        var expectedDirectory = Path.Combine(destinationRoot, settings.ImagesFolderName, "images");
        var expectedPath = Path.Combine(expectedDirectory, "icon.png");

        Assert.True(File.Exists(expectedPath));
        Assert.False(File.Exists(imagePath));
        Assert.Contains(statisticsService.MovedFiles, entry => entry.destination == expectedPath && entry.category == settings.ImagesFolderName);
    }

    private static void InvokeProcessFile(FileOrganizerService organizer, string filePath)
    {
        var method = typeof(FileOrganizerService)
            .GetMethod("ProcessFile", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ProcessFile method not found.");

        method.Invoke(organizer, new object[] { filePath });
    }

    private sealed class StubArchiveService : IArchiveService
    {
        public string HandleArchive(string archivePath, string destinationDirectory)
        {
            return FileUtilities.MoveFile(archivePath, destinationDirectory);
        }
    }

    private sealed class StubPhotoService : IPhotoService
    {
        public StubPhotoService(bool isPhoto)
        {
            _isPhoto = isPhoto;
        }

        private readonly bool _isPhoto;

        public bool IsPhoto(string filePath) => _isPhoto;

        public string MovePhoto(string filePath, string destinationRoot, string photosFolderName)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class RecordingStatisticsService : IStatisticsService
    {
        private readonly List<(string destination, string category)> _movedFiles = new();

        public IReadOnlyCollection<(string destination, string category)> MovedFiles => _movedFiles;

        public void RecordMovedFile(string destinationPath, string category)
        {
            _movedFiles.Add((destinationPath, category));
        }

        public void RecordMovedFiles(int count, string category)
        {
        }

        public void RecordMovedNonSplittableDirectory(INonSplittableDirectoryCategory category, string destinationPath, int fileCount)
        {
        }

        public void RecordDeletedDirectory(string directory)
        {
        }

        public void RecordUnknownFile(string extension)
        {
        }

        public void RecordDeletedFile(string extension)
        {
        }

        public void PrintStatistics(string sourceDirectory)
        {
        }
    }

    private sealed class StubMessageWriter : IMessageWriter
    {
        public void WriteLine(string message)
        {
        }
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
