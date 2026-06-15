using Domain.Library.EventHub.Abstractions;
using Domain.Library.Factory.Domain;
using Domain.Library.Factory.Domain.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Domain.Library.Configuration;

/// <summary>
///   Configures the domain factories for dependency injection.
/// </summary>
public static class DomainConfiguration
{
    /// <summary>
    /// Adds the domain factories to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the factories to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.TryAddSingleton<IDomainFactory, DomainFactory>();
        services.TryAddSingleton<IFactory, ActivatorFactory>();
        services.TryAddSingleton<IServiceFactory, ServiceFactory>();
        services.TryAddSingleton<IEventHub, EventHub.EventHub>();
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLogging(this IServiceCollection services)
    {
        return services;
    }
}
