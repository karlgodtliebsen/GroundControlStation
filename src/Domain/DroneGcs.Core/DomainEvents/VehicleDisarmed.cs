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

    /// <summary>
    /// Gets the ID of the vehicle that was disarmed.
    /// </summary>
    public VehicleId VehicleId => (VehicleId)Payload!;
}
