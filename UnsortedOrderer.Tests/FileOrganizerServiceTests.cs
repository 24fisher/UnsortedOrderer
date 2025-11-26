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
