using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Application.DependencyInjection;
using UnsortedOrderer.Categories;
using UnsortedOrderer.Contracts.Categories;
using UnsortedOrderer.Models;
using Xunit;

namespace UnsortedOrderer.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RepositoryCategory_IsCheckedBeforeFirmware()
    {
        var settings = new AppSettings(
            sourceDirectory: "source",
            destinationRoot: "destination",
            softFolderName: "Soft",
            archiveFolderName: "Archives",
            imagesFolderName: "Images",
            photosFolderName: "Photos",
            musicFolderName: "Music",
            musicalInstrumentsFolderName: "Instruments",
            eBooksFolderName: "EBooks",
            repositoriesFolderName: "Repositories",
            driversFolderName: "Drivers",
            firmwareFolderName: "Firmware",
            metadataFolderName: "Metadata",
            webFolderName: "Web",
            graphicsFolderName: "Graphics",
            unknownFolderName: "Unknown",
            deletedExtensions: Array.Empty<string>(),
            documentImageKeywords: Array.Empty<string>(),
            cameraFileNamePatterns: Array.Empty<DeviceBrandPattern>(),
            softwareArchiveKeywords: Array.Empty<string>());

        using var provider = new ServiceCollection()
            .AddUnsortedOrdererApplication(settings)
            .BuildServiceProvider();

        var categories = provider
            .GetRequiredService<IEnumerable<IFileCategory>>()
            .OfType<INonSplittableDirectoryCategory>()
            .ToArray();

        var repositoryIndex = Array.FindIndex(categories, category => category is RepositoriesCategory);
        var firmwareIndex = Array.FindIndex(categories, category => category is FirmwareCategory);

        Assert.NotEqual(-1, repositoryIndex);
        Assert.NotEqual(-1, firmwareIndex);
        Assert.True(repositoryIndex < firmwareIndex, "Repositories should be detected before firmware to avoid misclassifying code directories.");
    }
}
