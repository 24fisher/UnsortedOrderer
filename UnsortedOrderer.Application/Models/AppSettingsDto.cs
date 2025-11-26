namespace UnsortedOrderer.Models;

public sealed class AppSettingsDto
{
    public string? SourceDirectory { get; set; }

    public string? DestinationRoot { get; set; }

    public string? SoftFolderName { get; set; }

    public string? ArchiveFolderName { get; set; }

    public string? ImagesFolderName { get; set; }

    public string? PhotosFolderName { get; set; }

    public string? MusicFolderName { get; set; }

    public string? MusicalInstrumentsFolderName { get; set; }

    public string? EBooksFolderName { get; set; }

    public string? RepositoriesFolderName { get; set; }

    public string? DriversFolderName { get; set; }

    public string? FirmwareFolderName { get; set; }

    public string? MetadataFolderName { get; set; }

    public string? UnknownFolderName { get; set; }

    public string[]? DeletedExtensions { get; set; }

    public string[]? DocumentImageKeywords { get; set; }

    public string[]? SoftwareArchiveKeywords { get; set; }
}
