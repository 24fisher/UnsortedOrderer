using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnsortedOrderer.Core.Application;
using UnsortedOrderer.Core.DependencyInjection;
using UnsortedOrderer.Infrastructure.Mappers;
using UnsortedOrderer.Infrastructure.Services;
using UnsortedOrderer.Models;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = AppSettingsMapper.Map(configuration.Get<AppSettingsDto>() ?? new AppSettingsDto());

var services = new ServiceCollection();

services.AddSingleton<IMessageWriter, ConsoleMessageWriter>();
services.AddUnsortedOrdererCore(settings);

using var serviceProvider = services.BuildServiceProvider();

var application = serviceProvider.GetRequiredService<FileOrganizerApplication>();
application.Run();
