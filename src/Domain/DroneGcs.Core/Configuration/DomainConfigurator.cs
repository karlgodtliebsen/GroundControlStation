using Domain.Library.Factory.Domain.Abstractions;

using DroneGcs.Core.Commands;
using DroneGcs.Core.VehicleHandler;

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
        services.TryAddSingleton<IVehicleMessagePump, VehicleMessagePump>();
        services.TryAddSingleton<IVehicleConnectionMonitor, VehicleConnectionMonitor>();
        services.TryAddSingleton<IVehicleCommandService, VehicleCommandService>();
        services.TryAddSingleton<IHeartbeatVehicleHandler, HeartbeatVehicleHandler>();
        services.TryAddSingleton<IAttitudeVehicleHandler, AttitudeVehicleHandler>();
        services.TryAddSingleton<IBatteryVehicleHandler, BatteryVehicleHandler>();
        services.TryAddSingleton<IPositionVehicleHandler, PositionVehicleHandler>();

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
        domainFactory.Add<IAttitudeVehicleHandler, AttitudeVehicleHandler>();
        domainFactory.Add<IBatteryVehicleHandler, BatteryVehicleHandler>();
        domainFactory.Add<IPositionVehicleHandler, PositionVehicleHandler>();
        domainFactory.Add<IVehicleMessagePump, VehicleMessagePump>();
        return services;
    }
}
