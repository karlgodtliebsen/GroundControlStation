using Domain.Library.Configuration;

using DroneGcs.Core.Configuration;
using DroneGcs.Transport;
using DroneGcs.Transport.Configuration;

using DroneGs.MavLink.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace DroneGcs.Console.Configuration;

/// <summary>
/// 
/// </summary>
public static class ConsoleConfigurator
{
    /// <summary>
    /// Adds test configuration services to the service collection.
    /// </summary>
    public static IServiceCollection AddConfiguration()
    {
        var builder = new ConfigurationBuilder();
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = builder.Build();
        services.AddConfiguration(configuration);
        return services;
    }


    /// <summary>
    /// Adds MAVLink Transport services and dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which MAVLink services will be added.</param>
    /// <param name="configuration">The configuration to be used for MAVLink services.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddLibraryServices()
            .AddDomainServices(configuration)
            .AddMavLinkTransportServices(configuration)
            .AddMavLinkServices(configuration);

        //services.TryAddTransient<ITransportSmokeTestService, TransportSmokeTestService>();

        return services;
    }

    public static IServiceCollection AddDefaultLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });
        return services;
    }

    /// <summary>
    /// Configures test services and dependencies using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service provider to which test services will be added.</param>
    /// <returns>The updated service provider.</returns>
    public static IServiceProvider UseTestConfiguration(this IServiceProvider services)
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
