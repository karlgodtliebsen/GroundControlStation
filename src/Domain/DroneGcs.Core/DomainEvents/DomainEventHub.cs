using Domain.Library.EventHub;
using Domain.Library.EventHub.Events;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Represents a hub for publishing and subscribing to domain events.
/// </summary>
/// <param name="logger">The logger instance.</param>
public class DomainEventHub(ILogger<EventHub> logger) : EventHub(logger), IDomainEventHub
{
    /// <inheritdoc/>
    public virtual IDisposable SubscribeDomainEventAsync<T>(Func<IDomainEvent, CancellationToken, Task> handler) where T : IDomainEvent
    {
        var eventName = typeof(T).FullName!;
        return SubscribeAsync<IDomainEvent>(eventName, handler);
    }


    /// <inheritdoc/>
    public virtual IDisposable SubscribeDomainEvent<T>(Action<IDomainEvent> handler) where T : IDomainEvent
    {
        var eventName = typeof(T).FullName!;
        return Subscribe<IDomainEvent>(eventName, handler);
    }

    /// <inheritdoc/>
    public virtual async Task PublishDomainEventAsync(IDomainEvent data, CancellationToken cancellationToken = default)
    {
        var eventName = data.Name;
        var key = KeyGenerator.GetEventKey<IDomainEvent>(eventName);

        await PublishAsync<IDomainEvent>(key, eventName, data, cancellationToken);
        eventName = data.GetType().FullName!;
        await PublishAsync(eventName, data, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual void PublishDomainEvent(IDomainEvent data)
    {
        var eventName = data.Name;
        var key = KeyGenerator.GetEventKey<IDomainEvent>(eventName);

        Publish<IDomainEvent>(key, data);

        eventName = data.GetType().FullName!;
        Publish(eventName, data);
    }
}
