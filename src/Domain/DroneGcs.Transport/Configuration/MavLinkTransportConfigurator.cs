using Domain.Library.Factory.Domain.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DroneGcs.Transport.Configuration;

/// <summary>
/// 
/// </summary>
public static class MavLinkTransportConfigurator
{
    /// <summary>
    /// Adds MAVLink Transport services and dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which MAVLink Transport services will be added.</param>
    /// <param name="configuration"></param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMavLinkTransportConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IMavLinkTransport, UdpMavLinkTransport>();
        //UdpMavLinkTransport
        return services;
    }

    /// <summary>
    /// Configures and initializes MAVLink Transport services using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <returns>The updated service provider.</returns>
    public static IServiceProvider UseMavLinkTransportConfiguration(this IServiceProvider services)
    {
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        domainFactory.Add<IMavLinkTransport, UdpMavLinkTransport>();

        return services;
    }
}
