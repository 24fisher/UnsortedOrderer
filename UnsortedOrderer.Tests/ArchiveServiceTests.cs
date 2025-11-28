using System;
using System.IO;
using System.Collections.Generic;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;
using Xunit;

namespace UnsortedOrderer.Tests;

public sealed class ArchiveServiceTests
{
    [Fact]
    public void HandleArchive_ReplacesExistingFileAtDestination()
    {
        using var source = new TempDirectory();
        using var destinationRoot = new TempDirectory();

        var archiveName = "demo.zip";
        var archivePath = Path.Combine(source.Path, archiveName);
        var destinationDirectory = Path.Combine(destinationRoot.Path, "Archives");
        var existingArchivePath = Path.Combine(destinationDirectory, archiveName);

        Directory.CreateDirectory(destinationDirectory);
        File.WriteAllText(existingArchivePath, "old-archive");
        File.WriteAllText(archivePath, "new-archive");

        var settings = new AppSettings(
            source.Path,
            destinationRoot.Path,
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
            webFolderName: "Web",
            graphicsFolderName: "Graphics",
            unknownFolderName: "Unknown",
            deletedExtensions: Array.Empty<string>(),
            documentImageKeywords: Array.Empty<string>(),
            cameraFileNamePatterns: Array.Empty<DeviceBrandPattern>(),
            softwareArchiveKeywords: Array.Empty<string>());

        var statistics = new NullStatisticsService();
        var service = new ArchiveService(settings, Array.Empty<IFileCategory>(), statistics);

        var movedArchivePath = service.HandleArchive(archivePath, destinationDirectory);

        Assert.Equal(existingArchivePath, movedArchivePath);
        Assert.False(File.Exists(archivePath));
        Assert.True(File.Exists(existingArchivePath));
        Assert.Equal("new-archive", File.ReadAllText(existingArchivePath));
    }

    private sealed class NullStatisticsService : IStatisticsService
    {
        public void RecordMovedFile(string destinationPath, string category)
        {
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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
