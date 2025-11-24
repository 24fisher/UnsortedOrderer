using Microsoft.Extensions.Configuration;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.Get<AppSettings>() ?? new AppSettings();

Console.WriteLine("Loaded settings:");
Console.WriteLine($"  SourceDirectory: {settings.SourceDirectory}");
Console.WriteLine($"  DestinationRoot: {settings.DestinationRoot}");
Console.WriteLine($"  SoftFolderName: {settings.SoftFolderName}");
Console.WriteLine($"  DriversFolderName: {settings.DriversFolderName}");
Console.WriteLine($"  ArchiveFolderName: {settings.ArchiveFolderName}");
Console.WriteLine($"  ImagesFolderName: {settings.ImagesFolderName}");
Console.WriteLine($"  MusicFolderName: {settings.MusicFolderName}");
Console.WriteLine($"  MusicalInstrumentsFolderName: {settings.MusicalInstrumentsFolderName}");
Console.WriteLine($"  FirmwareFolderName: {settings.FirmwareFolderName}");
Console.WriteLine($"  MetadataFolderName: {settings.MetadataFolderName}");
Console.WriteLine();
Console.WriteLine("Starting organization...");

var categories = new IFileCategory[]
{
    new ImagesCategory(settings.ImagesFolderName),
    new MusicCategory(settings.MusicFolderName),
    new MusicalInstrumentsCategory(settings.MusicalInstrumentsFolderName),
    new DocumentsCategory(),
    new VideosCategory(),
    new ThreeDModelsCategory(),
    new ArchivesCategory(settings.ArchiveFolderName),
    new CertificatesCategory(),
    new FirmwareCategory(settings.FirmwareFolderName),
    new DriversCategory(settings.DriversFolderName),
    new MetadataCategory(settings.MetadataFolderName),
    new SoftCategory(settings.SoftFolderName),
    new UnknownCategory(settings.UnknownFolderName)
};

var organizer = new FileOrganizerService(
    settings,
    new SoftwareDistributivesDetector(),
    new ArchiveService(),
    new PhotoService(),
    categories);

organizer.Organize();
