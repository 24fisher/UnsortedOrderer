namespace UnsortedOrderer.Services;

public interface IArchiveService
{
    string HandleArchive(string archivePath, string destinationDirectory);
}
