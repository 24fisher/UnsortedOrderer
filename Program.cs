using Microsoft.Extensions.Configuration;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var settings = configuration.Get<AppSettings>() ?? new AppSettings();

Console.WriteLine("Loaded settings:");
Console.WriteLine($"  SourceDirectory: {settings.SourceDirectory}");
Console.WriteLine($"  DestinationRoot: {settings.DestinationRoot}");
Console.WriteLine($"  SoftFolderName: {settings.SoftFolderName}");
Console.WriteLine($"  ArchiveFolderName: {settings.ArchiveFolderName}");
Console.WriteLine($"  ImagesFolderName: {settings.ImagesFolderName}");
Console.WriteLine();
Console.WriteLine("Starting organization...");

var categories = new IFileCategory[]
{
    new ImagesCategory(settings.ImagesFolderName),
    new DocumentsCategory(),
    new VideosCategory(),
    new ThreeDModelsCategory(),
    new ArchivesCategory(settings.ArchiveFolderName),
    new CertificatesCategory(),
    new SoftCategory(settings.SoftFolderName)
};

var organizer = new FileOrganizerService(
    settings,
    new DistributionDetector(),
    new ArchiveService(),
    new PhotoService(),
    categories);

organizer.Organize();
