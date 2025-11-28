namespace UnsortedOrderer.Contracts.Services;

public interface IDownloadsCleanupService
{
    void CleanDownloadsIfRequested(string destinationPath);
}
