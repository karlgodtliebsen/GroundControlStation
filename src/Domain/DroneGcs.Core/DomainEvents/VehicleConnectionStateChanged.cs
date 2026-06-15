using Domain.Library.EventHub.Events;

using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle's connection state changes.
/// </summary>
public class VehicleConnectionStateChanged : DomainEvent<(VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt)>
{
    /// <inheritdoc />
    public VehicleConnectionStateChanged((VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt) data)
        : base("VehicleConnectionStateChanged", data)
    {
    }

    /// <inheritdoc />
    public VehicleConnectionStateChanged((VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt) data, MetaData md)
        : base("VehicleConnectionStateChanged", data, md)
    {
    }
}
