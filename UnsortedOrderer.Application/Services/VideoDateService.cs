using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.QuickTime;
using UnsortedOrderer.Contracts.Services;

using Directory = MetadataExtractor.Directory;

namespace UnsortedOrderer.Services;

public class VideoDateService : IVideoDateService
{
    public DateTime GetVideoDate(string filePath)
    {
        var metadataDate = TryGetMetadataDate(filePath);
        if (metadataDate.HasValue && metadataDate.Value.Year >= 1980)
        {
            return metadataDate.Value;
        }

        var fileTimestamp = TryGetFileTimestamp(filePath);
        if (fileTimestamp.HasValue)
        {
            return fileTimestamp.Value;
        }

        return DateTime.Now;
    }

    private static DateTime? TryGetFileTimestamp(string filePath)
    {
        try
        {
            var creationTime = File.GetCreationTime(filePath);
            if (creationTime.Year > 1)
            {
                return creationTime;
            }

            var lastWriteTime = File.GetLastWriteTime(filePath);
            if (lastWriteTime.Year > 1)
            {
                return lastWriteTime;
            }
        }
        catch
        {
            // ignored, will fall back to current time
        }

        return null;
    }

    protected virtual DateTime? TryGetMetadataDate(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            var quickTimeMovieHeader = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (TryGetDate(quickTimeMovieHeader, QuickTimeMovieHeaderDirectory.TagCreated, out var creationDate))
            {
                return creationDate;
            }

            var quickTimeDirectory = directories.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
            if (TryGetDate(quickTimeDirectory, QuickTimeMetadataHeaderDirectory.TagCreationDate, out creationDate))
            {
                return creationDate;
            }

            var fileSystemDirectory = directories.OfType<FileMetadataDirectory>().FirstOrDefault();
            if (TryGetDate(fileSystemDirectory, FileMetadataDirectory.TagFileModifiedDate, out creationDate))
            {
                return creationDate;
            }
        }
        catch
        {
            // ignored, fall back to file timestamps
        }

        return null;
    }

    private static bool TryGetDate(Directory? directory, int tag, out DateTime date)
    {
        date = default;

        if (directory?.TryGetDateTime(tag, out date) == true && date.Year > 1)
        {
            return true;
        }

        return false;
    }
}
