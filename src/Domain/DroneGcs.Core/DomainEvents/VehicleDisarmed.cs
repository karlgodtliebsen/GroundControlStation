using Domain.Library.EventHub.Events;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle is disarmed.
/// </summary>
public class VehicleDisarmed : DomainEvent<VehicleId>
{
    /// <inheritdoc />
    public VehicleDisarmed(VehicleId vehicleId)
        : base("VehicleDisarmed", vehicleId)
    {
    }

    /// <inheritdoc />
    public VehicleDisarmed(VehicleId vehicleId, MetaData md)
        : base("VehicleDisarmed", vehicleId, md)
    {
    }
}
