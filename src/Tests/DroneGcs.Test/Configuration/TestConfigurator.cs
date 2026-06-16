using Domain.Library.Configuration;

using DroneGcs.Core.Configuration;
using DroneGcs.Transport.Configuration;

using DroneGs.MavLink.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DroneGcs.Test.Configuration;

/// <summary>
/// 
/// </summary>
public static class TestConfigurator
{
    /// <summary>
    /// Adds test configuration services to the service collection.
    /// </summary>
    public static IServiceCollection AddTestConfiguration()
    {
        var builder = new ConfigurationBuilder();
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = builder.Build();
        services.AddTestConfiguration(configuration);
        return services;
    }


    /// <summary>
    /// Adds MAVLink Transport services and dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which MAVLink services will be added.</param>
    /// <param name="configuration">The configuration to be used for MAVLink services.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddTestConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddFactories()
            .AddDomainConfiguration(configuration)
            .AddMavLinkTransportConfiguration(configuration)
            .AddMavLinkConfiguration(configuration);

        return services;
    }


    /// <summary>
    /// Configures test services and dependencies using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service provider to which test services will be added.</param>
    /// <returns>The updated service provider.</returns>
    public static IServiceProvider UseTestConfiguration(this IServiceProvider services)
    {
        services
            .UseMavLinkConfiguration()
            .UseDomainConfiguration()
            ;
        return services;
    }
}
