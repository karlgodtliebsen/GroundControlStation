using Domain.Library.Configuration;

using DroneGcs.Core.Configuration;
using DroneGcs.Transport;
using DroneGcs.Transport.Configuration;

using DroneGs.MavLink.Configuration;

using GroundControlStationApp.Views.Connect;
using GroundControlStationApp.Views.Dashboard;
using GroundControlStationApp.Views.Messages;
using GroundControlStationApp.Views.Settings;
using GroundControlStationApp.Views.Vehicles;
using GroundControlStationApp.Views.Vehicles.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VehiclesView = GroundControlStationApp.Views.Vehicles.Views.VehiclesView;

namespace GroundControlStationApp.Configuration;

/// <summary>
/// Configures the application services and options.
/// </summary>
public static class ApplicationConfigurator
{
    /// <summary>
    /// Adds the application configuration to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        //TODO: add app-settings/config file
        //ApplicationOptions? applicationOptions = configuration.GetSection(ApplicationOptions.SectionName).Get<ApplicationOptions>();
        //ArgumentNullException.ThrowIfNull(applicationOptions, ApplicationOptions.Template);


        ApplicationOptions applicationOptions = new();

        // Configure ApplicationOptions using the options pattern
        services.Configure<ApplicationOptions>(options =>
        {
            options.BaudRate = applicationOptions.BaudRate;
            options.ConnectionType = applicationOptions.ConnectionType;
            options.Port = applicationOptions.Port;
        });

        ApplicationState state = new()
        {
            SelectedBaudRate = applicationOptions.BaudRate,
            SelectedConnectionType = applicationOptions.ConnectionType,
            SelectedPort = applicationOptions.Port
        };

        // Configure ApplicationState using the options pattern (for initial values)
        services.Configure<ApplicationState>(options =>
        {
            options.SelectedBaudRate = state.SelectedBaudRate;
            options.SelectedConnectionType = state.SelectedConnectionType;
            options.SelectedPort = state.SelectedPort;
        });

        // Register shared state service as singleton for runtime state management
        ApplicationStateService stateService = new();
        stateService.Initialize(state);
        services.TryAddSingleton(stateService);
        services.AddViewsConfiguration();
        services.TryAddSingleton<InitializeSitl>();
        services.TryAddSingleton<ModelMapper>();
        services.TryAddSingleton<AppViewModels.ThemeChangeViewModel>();

        services.TryAddSingleton(new CancellationTokenSource());


        services
            .AddLibraryServices()
            .AddDomainServices(configuration)
            .AddMavLinkTransportServices(configuration)
            .AddMavLinkServices(configuration);

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            loggingBuilder.SetMinimumLevel(LogLevel.Information);

            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
            loggingBuilder.AddFilter("System", LogLevel.Warning);
            //loggingBuilder.AddConsole();
            //loggingBuilder.Services.AddSingleton<ILoggerProvider, BufferedLoggerProvider>();

            loggingBuilder.AddDebug();
        });
        return services;
    }

    private static IServiceCollection AddViewsConfiguration(this IServiceCollection services)
    {
        services.TryAddSingleton<App>();
        services.TryAddSingleton<AppShell>();

        services.TryAddSingleton<MainPage>();
        services.TryAddSingleton<MainPageViewModel>();

        //SubView/Controls
        services.TryAddSingleton<ConnectPopup>();
        services.TryAddSingleton<ConnectPopupViewModel>();

        services.TryAddTransient<VehiclesViewModel>();
        services.TryAddTransient<VehiclesView>();

        services.TryAddSingleton<VehiclesPageViewModel>();
        services.TryAddSingleton<VehiclesPage>();

        services.TryAddSingleton<SettingsPageViewModel>();
        services.TryAddSingleton<SettingsPage>();

        services.TryAddSingleton<MessagesPageViewModel>();
        services.TryAddSingleton<MessagesPage>();

        services.TryAddSingleton<DashboardPageViewModel>();
        services.TryAddSingleton<DashboardPage>();

        return services;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IServiceProvider UseApplication(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationOptions>>();
        logger.LogInformation("UseApplication - Setting up Application Operations");

        var endPoint = serviceProvider.GetRequiredService<IOptions<TransportEndpoint>>();

        endPoint.Value.RemoteHost = "127.0.0.1";
        endPoint.Value.RemotePort = 14551;

        endPoint.Value.LocalHost = "0.0.0.0";
        endPoint.Value.LocalPort = 14550;
        logger.LogInformation($"Console configuration initialized. UDP local:  {endPoint.Value.LocalHost}:{endPoint.Value.LocalPort}");
        logger.LogInformation($"Console configuration initialized. UDP remote: {endPoint.Value.RemoteHost}:{endPoint.Value.RemotePort}");

        //serviceProvider.GetRequiredService<MessagesPageViewModel>().SetupSubscriptions();
        //serviceProvider.GetRequiredService<DashboardPageViewModel>().SetupSubscriptions();

        //serviceProvider.GetRequiredService<DashboardPage>();
        //serviceProvider.GetRequiredService<SettingsPage>();
        //serviceProvider.GetRequiredService<MessagesPage>();


        serviceProvider.GetRequiredService<VehiclesPage>();
        //serviceProvider.GetRequiredService<VehiclesView>();

        serviceProvider
            .UseMavLinkServices()
            .UseDomainServices()
            ;

        return serviceProvider;
    }
}
