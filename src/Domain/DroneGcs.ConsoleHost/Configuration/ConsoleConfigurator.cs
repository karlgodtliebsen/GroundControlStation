using Domain.Library.Configuration;

using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;
using DroneGcs.Core.Configuration;
using DroneGcs.Transport;
using DroneGcs.Transport.Configuration;

using DroneGs.MavLink.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneGcs.ConsoleHost.Configuration;

/// <summary>
/// 
/// </summary>
public static class ConsoleConfigurator
{
    /// <summary>
    /// Adds services and dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which services will be added.</param>
    /// <param name="configuration">The configuration to be used for services.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<IMultiTaskWorkerService, MultiTaskWorkerService>();
        services.TryAddTransient<ISingleTaskWorkerService, CommandWorkerService>();
        services.TryAddSingleton<LogBuffer>();

        //services.TryAddTransient<MultiTaskHostedService>();
        //services.TryAddTransient<SingleTaskHostedService>();
        services.AddHostedService<MultiTaskHostedService>();
        services.AddHostedService<SingleTaskHostedService>();
        services.AddHostedService<DashboardService>();


        services
            .AddLibraryServices()
            .AddDomainServices(configuration)
            .AddMavLinkTransportServices(configuration)
            .AddMavLinkServices(configuration);
        return services;
    }

    public static IServiceCollection AddDefaultLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CommandOutputBuffer>();
        services.AddSingleton<LoggingOutputBuffer>();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            loggingBuilder.SetMinimumLevel(LogLevel.Information);

            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
            loggingBuilder.AddFilter("System", LogLevel.Warning);
            //loggingBuilder.AddConsole();

            loggingBuilder.Services.AddSingleton<ILoggerProvider, BufferedLoggerProvider>();

            loggingBuilder.AddDebug();
        });
        return services;
    }

    /// <summary>
    /// Configures services and dependencies using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service provider to which configuration services will be added.</param>
    /// <returns>The updated service provider.</returns>
    public static IServiceProvider UseConfiguration(this IServiceProvider services)
    {
        var endPoint = services.GetRequiredService<IOptions<TransportEndpoint>>();

        var logger = services.GetRequiredService<ILogger<ServiceProvider>>();
        endPoint.Value.RemoteHost = "127.0.0.1";
        endPoint.Value.RemotePort = 14551;

        endPoint.Value.LocalHost = "0.0.0.0";
        endPoint.Value.LocalPort = 14550;
        logger.LogInformation($"Console configuration initialized. UDP local:  {endPoint.Value.LocalHost}:{endPoint.Value.LocalPort}");
        logger.LogInformation($"Console configuration initialized. UDP remote: {endPoint.Value.RemoteHost}:{endPoint.Value.RemotePort}");


        services
            .UseMavLinkServices()
            .UseDomainServices()
            ;
        return services;
    }
}
