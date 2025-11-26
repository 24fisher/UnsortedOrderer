namespace UnsortedOrderer.Contracts.Services;

public interface IArchiveService
{
    string HandleArchive(string archivePath, string destinationDirectory);
}
