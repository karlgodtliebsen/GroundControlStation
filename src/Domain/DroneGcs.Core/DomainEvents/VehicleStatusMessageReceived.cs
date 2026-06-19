using Domain.Library.EventHub.Events;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle status message is received, which may indicate a change in the vehicle's connection state or other status information.
/// </summary>
public class VehicleStatusMessageReceived : DomainEvent<(VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt)>
{
    /// <inheritdoc />
    public VehicleStatusMessageReceived((VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt) data)
        : base("VehicleStatusMessageReceived", data)
    {
    }

    /// <inheritdoc />
    public VehicleStatusMessageReceived((VehicleId VehicleId, VehicleConnectionState PreviousState, VehicleConnectionState CurrentState, DateTimeOffset ChangedAt) data, MetaData md)
        : base("VehicleStatusMessageReceived", data, md)
    {
    }
}
