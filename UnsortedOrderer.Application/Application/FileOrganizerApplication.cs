using UnsortedOrderer.Models;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Application.Application;

public sealed class FileOrganizerApplication
{
    private readonly AppSettings _settings;
    private readonly FileOrganizerService _organizer;
    private readonly IMessageWriter _messageWriter;

    public FileOrganizerApplication(AppSettings settings, FileOrganizerService organizer, IMessageWriter messageWriter)
    {
        _settings = settings;
        _organizer = organizer;
        _messageWriter = messageWriter;
    }

    public void Run()
    {
        var deletionExtensions = FileUtilities
            .NormalizeExtensions(_settings.DeletedExtensions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (deletionExtensions.Length == 0)
        {
            _messageWriter.WriteLine("WARNING: No file extensions configured for deletion in appsettings.json.");
        }
        else
        {
            _messageWriter.WriteLine("WARNING: Files with these extensions will be deleted: " + string.Join(", ", deletionExtensions));
        }

        _messageWriter.WriteLine(string.Empty);

        _messageWriter.WriteLine("Loaded settings:");
        _messageWriter.WriteLine($"  SourceDirectory: {_settings.SourceDirectory}");
        _messageWriter.WriteLine($"  DestinationRoot: {_settings.DestinationRoot}");
        _messageWriter.WriteLine($"  SoftFolderName: {_settings.SoftFolderName}");
        _messageWriter.WriteLine($"  ArchiveFolderName: {_settings.ArchiveFolderName}");
        _messageWriter.WriteLine($"  ImagesFolderName: {_settings.ImagesFolderName}");
        _messageWriter.WriteLine($"  PhotosFolderName: {_settings.PhotosFolderName}");
        _messageWriter.WriteLine($"  MusicFolderName: {_settings.MusicFolderName}");
        _messageWriter.WriteLine($"  MusicalInstrumentsFolderName: {_settings.MusicalInstrumentsFolderName}");
        _messageWriter.WriteLine($"  EBooksFolderName: {_settings.EBooksFolderName}");
        _messageWriter.WriteLine($"  RepositoriesFolderName: {_settings.RepositoriesFolderName}");
        _messageWriter.WriteLine($"  DriversFolderName: {_settings.DriversFolderName}");
        _messageWriter.WriteLine($"  FirmwareFolderName: {_settings.FirmwareFolderName}");
        _messageWriter.WriteLine($"  MetadataFolderName: {_settings.MetadataFolderName}");
        _messageWriter.WriteLine(string.Empty);
        _messageWriter.WriteLine("Starting organization...");

        _organizer.Organize();
    }
}
