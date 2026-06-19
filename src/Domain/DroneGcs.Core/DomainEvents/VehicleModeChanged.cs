using Domain.Library.EventHub.Events;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle's mode changes, which may indicate a change in the vehicle's operational state (e.g., from manual to autonomous mode).
/// </summary>
public class VehicleModeChanged : DomainEvent<(VehicleId VehicleId, VehicleMode mode, DateTimeOffset ChangedAt)>
{
    /// <inheritdoc />
    public VehicleModeChanged((VehicleId VehicleId, VehicleMode mode, DateTimeOffset ChangedAt) data)
        : base("VehicleModeChanged", data)
    {
    }

    /// <inheritdoc />
    public VehicleModeChanged((VehicleId VehicleId, VehicleMode mode, DateTimeOffset ChangedAt) data, MetaData md)
        : base("VehicleModeChanged", data, md)
    {
    }
}
