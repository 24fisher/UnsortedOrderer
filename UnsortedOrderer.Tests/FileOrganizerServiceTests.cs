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

    private static void InvokeProcessFile(FileOrganizerService organizer, string filePath)
    {
        var method = typeof(FileOrganizerService)
            .GetMethod("ProcessFile", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ProcessFile method not found.");

        method.Invoke(organizer, new object[] { filePath });
    }

    private static void InvokeProcessDirectory(FileOrganizerService organizer, string directoryPath)
    {
        var method = typeof(FileOrganizerService)
            .GetMethod("ProcessDirectory", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ProcessDirectory method not found.");

        method.Invoke(organizer, new object[] { directoryPath });
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

    private sealed class StubMessengerPathService : IMessengerPathService
    {
        public string? GetMessengerFolder(string filePath) => null;
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

    [Fact]
    public void RepositoryInsideWrapperDirectories_IsExtractedAndMoved()
    {
        using var source = new TempDirectory();
        using var destination = new TempDirectory();

        var outer = System.IO.Path.Combine(source.Path, "outer");
        var inner = System.IO.Path.Combine(outer, "inner");
        var repositoryRoot = System.IO.Path.Combine(inner, "my-repo");

        Directory.CreateDirectory(repositoryRoot);
        Directory.CreateDirectory(System.IO.Path.Combine(repositoryRoot, ".git"));
        File.WriteAllText(System.IO.Path.Combine(repositoryRoot, "Program.cs"), "class Program { }");

        var settings = new AppSettings(
            source.Path,
            destination.Path,
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
            unknownFolderName: "Unknown",
            deletedExtensions: Array.Empty<string>(),
            documentImageKeywords: Array.Empty<string>(),
            cameraFileNamePatterns: Array.Empty<DeviceBrandPattern>(),
            softwareArchiveKeywords: Array.Empty<string>());

        var categories = new IFileCategory[]
        {
            new RepositoriesCategory(settings.RepositoriesFolderName),
            new UnknownCategory(settings.UnknownFolderName)
        };

        var organizer = new FileOrganizerService(
            settings,
            new StubArchiveService(),
            new StubPhotoService(isPhoto: false),
            new StubMessengerPathService(),
            categories,
            new RecordingStatisticsService(),
            new StubMessageWriter());

        InvokeProcessDirectory(organizer, source.Path);

        var repositoriesDestination = System.IO.Path.Combine(destination.Path, settings.RepositoriesFolderName);
        var expectedRepositoryPath = System.IO.Path.Combine(repositoriesDestination, "my-repo");

        Assert.True(Directory.Exists(expectedRepositoryPath));
        Assert.False(Directory.Exists(System.IO.Path.Combine(repositoriesDestination, "outer")));
        Assert.True(Directory.Exists(outer));
    }
}
