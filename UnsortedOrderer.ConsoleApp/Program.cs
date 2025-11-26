using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Contracts.Services;
using UnsortedOrderer.Application.Application;
using UnsortedOrderer.Application.DependencyInjection;
using UnsortedOrderer.Infrastructure.Mappers;
using UnsortedOrderer.Models;
using UnsortedOrderer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = AppSettingsMapper.Map(configuration.Get<AppSettingsDto>() ?? new AppSettingsDto());

var services = new ServiceCollection();

services.AddSingleton<IMessageWriter, ConsoleMessageWriter>();
services.AddUnsortedOrdererApplication(settings);

using var serviceProvider = services.BuildServiceProvider();

serviceProvider
    .GetRequiredService<IDesktopCleanupService>()
    .CleanIfRunningFromDesktop(settings.SourceDirectory);

var application = serviceProvider.GetRequiredService<FileOrganizerApplication>();
application.Run();
