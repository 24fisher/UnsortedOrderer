namespace UnsortedOrderer.Models;

public sealed class AppSettings
{
    public string SourceDirectory { get; set; } = string.Empty;

    public string DestinationRoot { get; set; } = string.Empty;

    public string SoftFolderName { get; set; } = "Soft";

    public string DriversFolderName { get; set; } = "Drivers";

    public string ArchiveFolderName { get; set; } = "Archives";

    public string ImagesFolderName { get; set; } = "Images";

    public string MusicFolderName { get; set; } = "Music";

    public string MusicalInstrumentsFolderName { get; set; } = "Instruments";

    public string FirmwareFolderName { get; set; } = "Firmware";

    public string UnknownFolderName { get; set; } = "_Unknown";
}
