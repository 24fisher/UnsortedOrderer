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
        string firmwareFolderName,
        string metadataFolderName,
        string unknownFolderName,
        string[] deletedExtensions)
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
        FirmwareFolderName = firmwareFolderName;
        MetadataFolderName = metadataFolderName;
        UnknownFolderName = unknownFolderName;
        DeletedExtensions = deletedExtensions;
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

    public string FirmwareFolderName { get; }

    public string MetadataFolderName { get; }

    public string UnknownFolderName { get; }

    public string[] DeletedExtensions { get; }
}
