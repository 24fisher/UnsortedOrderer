using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

namespace UnsortedOrderer.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnsortedOrdererApplication(this IServiceCollection services, AppSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IArchiveService, ArchiveService>();
        services.AddSingleton<ICameraFileNamePatternService>(
            _ => new CameraFileNamePatternService(settings.CameraFileNamePatterns));
        services.AddSingleton<IPhotoService, PhotoService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IEnumerable<IFileCategory>>(provider =>
        {
            var appSettings = provider.GetRequiredService<AppSettings>();
            var cameraFileNamePatternService = provider.GetRequiredService<ICameraFileNamePatternService>();
            return new IFileCategory[]
            {
                new PhotosCategory(appSettings.PhotosFolderName),
                new ImagesCategory(appSettings.ImagesFolderName),
                new MusicCategory(appSettings.MusicFolderName),
                new MusicalInstrumentsCategory(appSettings.MusicalInstrumentsFolderName),
                new EBooksCategory(appSettings.EBooksFolderName),
                new DocumentsCategory(),
                new VideosCategory(cameraFileNamePatternService),
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
        services.AddSingleton<FileOrganizerApplication>();

        return services;
    }
}
