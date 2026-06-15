using Domain.Library.EventHub.Abstractions;
using Domain.Library.EventHub.Events;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Defines the contract for an event hub that allows subscribing to and publishing events. 
/// </summary>
public interface IDomainEventHub : IEventHub
{
    /// <summary>
    /// Subscribes For the DomainEvent specified Func signature
    /// </summary>
    /// <param name="handler"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IDisposable SubscribeDomainEventAsync<T>(Func<IDomainEvent, CancellationToken, Task> handler) where T : IDomainEvent;

    /// <summary>
    /// Subscribes For the DomainEvent specified Func signature
    /// </summary>
    /// <param name="handler"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IDisposable SubscribeDomainEvent<T>(Action<IDomainEvent> handler) where T : IDomainEvent;

    /// <summary>
    /// Publish DomainEvent Async
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishDomainEventAsync(IDomainEvent data, CancellationToken cancellationToken = default);


    /// <summary>
    /// Publish DomainEvent
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    void PublishDomainEvent(IDomainEvent data);
}
