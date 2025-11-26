namespace UnsortedOrderer.Models;

public sealed class AppSettings
{
    public AppSettings(
        string sourceDirectory,
        string destinationRoot,
        string softFolderName,
        string archiveFolderName,
        string imagesFolderName,
        string photosFolderName,
        string musicFolderName,
        string musicalInstrumentsFolderName,
        string eBooksFolderName,
        string repositoriesFolderName,
        string driversFolderName,
        string firmwareFolderName,
        string metadataFolderName,
        string unknownFolderName,
        string[] deletedExtensions,
        string[] documentImageKeywords,
        IReadOnlyCollection<DeviceBrandPattern> cameraFileNamePatterns)
        string[] softwareArchiveKeywords)
    {
        SourceDirectory = sourceDirectory;
        DestinationRoot = destinationRoot;
        SoftFolderName = softFolderName;
        ArchiveFolderName = archiveFolderName;
        ImagesFolderName = imagesFolderName;
        PhotosFolderName = photosFolderName;
        MusicFolderName = musicFolderName;
        MusicalInstrumentsFolderName = musicalInstrumentsFolderName;
        EBooksFolderName = eBooksFolderName;
        RepositoriesFolderName = repositoriesFolderName;
        DriversFolderName = driversFolderName;
        FirmwareFolderName = firmwareFolderName;
        MetadataFolderName = metadataFolderName;
        UnknownFolderName = unknownFolderName;
        DeletedExtensions = deletedExtensions;
        DocumentImageKeywords = documentImageKeywords;
        CameraFileNamePatterns = cameraFileNamePatterns;
        SoftwareArchiveKeywords = softwareArchiveKeywords;
    }

    public string SourceDirectory { get; }

    public string DestinationRoot { get; }

    public string SoftFolderName { get; }

    public string ArchiveFolderName { get; }

    public string ImagesFolderName { get; }

    public string PhotosFolderName { get; }

    public string MusicFolderName { get; }

    public string MusicalInstrumentsFolderName { get; }

    public string EBooksFolderName { get; }

    public string RepositoriesFolderName { get; }

    public string DriversFolderName { get; }

    public string FirmwareFolderName { get; }

    public string MetadataFolderName { get; }

    public string UnknownFolderName { get; }

    public string[] DeletedExtensions { get; }

    public string[] DocumentImageKeywords { get; }

    public IReadOnlyCollection<DeviceBrandPattern> CameraFileNamePatterns { get; }
    public string[] SoftwareArchiveKeywords { get; }
}
