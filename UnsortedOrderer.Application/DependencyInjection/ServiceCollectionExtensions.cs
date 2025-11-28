using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Application.Application;
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
            _ => new PhotoCameraFileNamePatternService(settings.CameraFileNamePatterns));
        services.AddSingleton<ICameraFileNamePatternService>(
            _ => new VideoCameraFileNamePatternService(settings.CameraFileNamePatterns));
        services.AddSingleton<IMessengerPathService, MessengerPathService>();
        services.AddSingleton<IDesktopCleanupService, DesktopCleanupService>();
        services.AddSingleton<IDownloadsCleanupService, DownloadsCleanupService>();
        services.AddSingleton<IPhotoCameraMetadataService, PhotoCameraMetadataService>();
        services.AddSingleton<IVideoDateService, VideoDateService>();
        services.AddSingleton<IPhotoService, PhotoService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IMusicDirectoryDetector, MusicDirectoryDetector>();
        services.AddSingleton<RepositoryDetector>(
            _ => new RepositoryDetector(RepositoriesCategory.CodeExtensions));
        services.AddSingleton<SoftwareDistributivesDetector>();

        services.AddSingleton<IFileCategoryParsingService>(
            provider => (IFileCategoryParsingService)provider.GetRequiredService<IPhotoService>());
        services.AddSingleton<IFileCategoryParsingService>(
            provider => (IFileCategoryParsingService)provider.GetRequiredService<IMusicDirectoryDetector>());
        services.AddSingleton<IFileCategoryParsingService>(
            provider => provider.GetRequiredService<RepositoryDetector>());
        services.AddSingleton<IFileCategoryParsingService>(
            provider => provider.GetRequiredService<SoftwareDistributivesDetector>());
        services.AddSingleton<IEnumerable<IFileCategory>>(provider =>
        {
            var appSettings = provider.GetRequiredService<AppSettings>();
            var cameraFileNamePatternService = provider.GetRequiredService<IEnumerable<ICameraFileNamePatternService>>();
            var messengerPathService = provider.GetRequiredService<IMessengerPathService>();
            var videoDateService = provider.GetRequiredService<IVideoDateService>();
            var musicDirectoryDetector = provider.GetRequiredService<IMusicDirectoryDetector>();
            var repositoryDetector = provider.GetRequiredService<RepositoryDetector>();
            var softwareDistributivesDetector = provider.GetRequiredService<SoftwareDistributivesDetector>();
            return new IFileCategory[]
            {
                new PhotosCategory(appSettings.PhotosFolderName),
                new ImagesCategory(appSettings.ImagesFolderName),
                new MusicCategory(appSettings.MusicFolderName, musicDirectoryDetector),
                new MusicalInstrumentsCategory(appSettings.MusicalInstrumentsFolderName),
                new EBooksCategory(appSettings.EBooksFolderName),
                new DocumentsCategory(),
                new WebCategory(appSettings.WebFolderName),
                new GraphicsCategory(appSettings.GraphicsFolderName),
                new VideosCategory(cameraFileNamePatternService, videoDateService, messengerPathService),
                new ThreeDModelsCategory(),
                new ArchivesCategory(appSettings.ArchiveFolderName),
                new CertificatesCategory(),
                new RepositoriesCategory(appSettings.RepositoriesFolderName, repositoryDetector),
                new FirmwareCategory(appSettings.FirmwareFolderName),
                new MetadataCategory(appSettings.MetadataFolderName),
                new DriversCategory(appSettings.DriversFolderName),
                new SoftCategory(appSettings.SoftFolderName, softwareDistributivesDetector),
                new UnknownCategory(appSettings.UnknownFolderName)
            };
        });
        services.AddSingleton<FileOrganizerService>();
        services.AddSingleton<FileOrganizerApplication>();

        return services;
    }
}
