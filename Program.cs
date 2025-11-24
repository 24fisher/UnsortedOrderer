using Microsoft.Extensions.Configuration;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.Get<AppSettings>() ?? new AppSettings();
var deletionExtensions = FileUtilities
    .NormalizeExtensions(settings.DeletedExtensions)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (deletionExtensions.Length == 0)
{
    Console.WriteLine("WARNING: No file extensions configured for deletion.");
}
else
{
    Console.WriteLine("WARNING: Files with these extensions will be deleted: " + string.Join(", ", deletionExtensions));
}
Console.WriteLine();

Console.WriteLine("Loaded settings:");
Console.WriteLine($"  SourceDirectory: {settings.SourceDirectory}");
Console.WriteLine($"  DestinationRoot: {settings.DestinationRoot}");
Console.WriteLine($"  SoftFolderName: {settings.SoftFolderName}");
Console.WriteLine($"  ArchiveFolderName: {settings.ArchiveFolderName}");
Console.WriteLine($"  ImagesFolderName: {settings.ImagesFolderName}");
Console.WriteLine($"  PhotosFolderName: {settings.PhotosFolderName}");
Console.WriteLine($"  MusicFolderName: {settings.MusicFolderName}");
Console.WriteLine($"  MusicalInstrumentsFolderName: {settings.MusicalInstrumentsFolderName}");
Console.WriteLine($"  EBooksFolderName: {settings.EBooksFolderName}");
Console.WriteLine($"  FirmwareFolderName: {settings.FirmwareFolderName}");
Console.WriteLine($"  MetadataFolderName: {settings.MetadataFolderName}");
Console.WriteLine();
Console.WriteLine("Starting organization...");

var categories = new IFileCategory[]
{
    new PhotosCategory(settings.PhotosFolderName),
    new ImagesCategory(settings.ImagesFolderName),
    new MusicCategory(settings.MusicFolderName),
    new MusicalInstrumentsCategory(settings.MusicalInstrumentsFolderName),
    new EBooksCategory(settings.EBooksFolderName),
    new DocumentsCategory(),
    new VideosCategory(),
    new ThreeDModelsCategory(),
    new ArchivesCategory(settings.ArchiveFolderName),
    new CertificatesCategory(),
    new FirmwareCategory(settings.FirmwareFolderName),
    new MetadataCategory(settings.MetadataFolderName),
    new SoftCategory(settings.SoftFolderName),
    new UnknownCategory(settings.UnknownFolderName)
};

var organizer = new FileOrganizerService(
    settings,
    new ArchiveService(),
    new PhotoService(),
    categories);

organizer.Organize();
