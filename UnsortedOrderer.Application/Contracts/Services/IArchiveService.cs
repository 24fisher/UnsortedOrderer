namespace UnsortedOrderer.Contracts.Services;

public interface IArchiveService
{
    ArchiveHandlingResult HandleArchiveFile(string archivePath, string categoryName);

    string HandleArchive(string archivePath, string destinationDirectory);
}

public sealed record ArchiveHandlingResult(string DestinationPath, string CategoryName);
