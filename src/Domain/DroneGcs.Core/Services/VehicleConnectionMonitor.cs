using Domain.Library.DateTime.Domain;

using DroneGcs.Core.DomainEvents;

namespace DroneGcs.Core.Services;

/// <summary>
/// Monitors the connection state of vehicles.
/// </summary>
/// <param name="vehicleRegistry">The vehicle registry to monitor.</param>
/// <param name="clock">The clock to use for time-based calculations.</param>
/// <param name="eventHub">The event hub to publish domain events.</param>
public sealed class VehicleConnectionMonitor(IVehicleRegistry vehicleRegistry, IDateTimeProvider clock, IDomainEventHub eventHub) : IVehicleConnectionMonitor
{
    private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan OfflineAfter = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Updates the connection states of all monitored vehicles.
    /// </summary>
    public void UpdateConnectionStates()
    {
        vehicleRegistry.UpdateConnectionStates(clock.UtcNow, StaleAfter, OfflineAfter);
    }
}
