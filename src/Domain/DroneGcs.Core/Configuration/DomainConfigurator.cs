using Domain.Library.Factory.Domain.Abstractions;

using DroneGcs.Core.MavLink;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DroneGcs.Core.Configuration;

/// <summary>
/// 
/// </summary>
public static class DomainConfigurator
{
    /// <summary>
    /// Adds domain services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which domain services will be added.</param>
    /// <param name="configuration"></param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IVehicleRegistry, VehicleRegistry>();
        services.TryAddSingleton<IHeartbeatVehicleHandler, HeartbeatVehicleHandler>();
        services.TryAddSingleton<IVehicleMessagePump, VehicleMessagePump>();
        services.TryAddSingleton<IVehicleConnectionMonitor, VehicleConnectionMonitor>();

        return services;
    }

    /// <summary>
    /// Configures domain services using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service provider to which domain services will be added.</param>
    /// <returns>The updated service provider.</returns>
    public static IServiceProvider UseDomainConfiguration(this IServiceProvider services)
    {
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        domainFactory.Add<IHeartbeatVehicleHandler, HeartbeatVehicleHandler>();
        domainFactory.Add<IVehicleMessagePump, VehicleMessagePump>();
        return services;
    }
    //IMavLinkClient
}
