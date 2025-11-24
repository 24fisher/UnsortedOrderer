namespace UnsortedOrderer.Services;

public interface IArchiveService
{
    string HandleArchive(string archivePath, string destinationDirectory);
}

public sealed class ArchiveService : IArchiveService
{
    public string HandleArchive(string archivePath, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(archivePath));
        var finalPath = FileUtilities.GetUniqueFilePath(destinationPath);
        File.Move(archivePath, finalPath);

        return finalPath;
    }
}
