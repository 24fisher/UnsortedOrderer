namespace UnsortedOrderer.Services;

public interface IArchiveService
{
    string HandleArchive(string archivePath, string destinationRoot, string archiveFolderName, string softFolderName);
}

public sealed class ArchiveService : IArchiveService
{
    public string HandleArchive(string archivePath, string destinationRoot, string archiveFolderName, string softFolderName)
    {
        var archiveName = Path.GetFileNameWithoutExtension(archivePath) ?? string.Empty;

        var archiveDestinationDirectory = Path.Combine(destinationRoot, archiveFolderName);
        Directory.CreateDirectory(archiveDestinationDirectory);

        var destinationPath = Path.Combine(archiveDestinationDirectory, Path.GetFileName(archivePath));
        var finalPath = FileUtilities.GetUniqueFilePath(destinationPath);
        File.Move(archivePath, finalPath);

        return finalPath;
    }
}
