using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = AppSettingsMapper.Map(configuration.Get<AppSettingsDto>() ?? new AppSettingsDto());
var deletionExtensions = FileUtilities
    .NormalizeExtensions(settings.DeletedExtensions)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

var services = new ServiceCollection();

services.AddSingleton(settings);
services.AddSingleton<IMessageWriter, ConsoleMessageWriter>();
services.AddSingleton<IArchiveService, ArchiveService>();
services.AddSingleton<IPhotoService, PhotoService>();
services.AddSingleton<IEnumerable<IFileCategory>>(provider =>
{
    var appSettings = provider.GetRequiredService<AppSettings>();
    return new IFileCategory[]
    {
        new PhotosCategory(appSettings.PhotosFolderName),
        new ImagesCategory(appSettings.ImagesFolderName),
        new MusicCategory(appSettings.MusicFolderName),
        new MusicalInstrumentsCategory(appSettings.MusicalInstrumentsFolderName),
        new EBooksCategory(appSettings.EBooksFolderName),
        new DocumentsCategory(),
        new VideosCategory(),
        new ThreeDModelsCategory(),
        new ArchivesCategory(appSettings.ArchiveFolderName),
        new CertificatesCategory(),
        new FirmwareCategory(appSettings.FirmwareFolderName),
        new MetadataCategory(appSettings.MetadataFolderName),
        new DriversCategory(appSettings.DriversFolderName),
        new RepositoriesCategory(appSettings.RepositoriesFolderName),
        new SoftCategory(appSettings.SoftFolderName),
        new UnknownCategory(appSettings.UnknownFolderName)
    };
});
services.AddSingleton<FileOrganizerService>();

using var serviceProvider = services.BuildServiceProvider();

var messageWriter = serviceProvider.GetRequiredService<IMessageWriter>();

if (deletionExtensions.Length == 0)
{
    messageWriter.WriteLine("WARNING: No file extensions configured for deletion in appsettings.json.");
}
else
{
    messageWriter.WriteLine("WARNING: Files with these extensions will be deleted: " + string.Join(", ", deletionExtensions));
}
messageWriter.WriteLine(string.Empty);

messageWriter.WriteLine("Loaded settings:");
messageWriter.WriteLine($"  SourceDirectory: {settings.SourceDirectory}");
messageWriter.WriteLine($"  DestinationRoot: {settings.DestinationRoot}");
messageWriter.WriteLine($"  SoftFolderName: {settings.SoftFolderName}");
messageWriter.WriteLine($"  ArchiveFolderName: {settings.ArchiveFolderName}");
messageWriter.WriteLine($"  ImagesFolderName: {settings.ImagesFolderName}");
messageWriter.WriteLine($"  PhotosFolderName: {settings.PhotosFolderName}");
messageWriter.WriteLine($"  MusicFolderName: {settings.MusicFolderName}");
messageWriter.WriteLine($"  MusicalInstrumentsFolderName: {settings.MusicalInstrumentsFolderName}");
messageWriter.WriteLine($"  EBooksFolderName: {settings.EBooksFolderName}");
messageWriter.WriteLine($"  RepositoriesFolderName: {settings.RepositoriesFolderName}");
messageWriter.WriteLine($"  DriversFolderName: {settings.DriversFolderName}");
messageWriter.WriteLine($"  FirmwareFolderName: {settings.FirmwareFolderName}");
messageWriter.WriteLine($"  MetadataFolderName: {settings.MetadataFolderName}");
messageWriter.WriteLine(string.Empty);
messageWriter.WriteLine("Starting organization...");

var organizer = serviceProvider.GetRequiredService<FileOrganizerService>();
organizer.Organize();
