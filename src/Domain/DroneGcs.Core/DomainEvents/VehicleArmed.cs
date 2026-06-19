using Domain.Library.EventHub.Events;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle is armed 
/// </summary>
public class VehicleArmed : DomainEvent<VehicleId>
{
    /// <inheritdoc />
    public VehicleArmed(VehicleId vehicleId)
        : base("VehicleArmed", vehicleId)
    {
    }

    /// <inheritdoc />
    public VehicleArmed(VehicleId vehicleId, MetaData md)
        : base("VehicleArmed", vehicleId, md)
    {
    }
}
