using Microsoft.Extensions.Configuration;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.Get<AppSettings>() ?? new AppSettings();

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
