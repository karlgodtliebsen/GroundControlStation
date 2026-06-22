using Domain.Library.EventHub.Events;

using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle is registered.
/// </summary>
public class VehicleRegistered : DomainEvent<VehicleId>
{
    /// <summary>
    /// 
    /// </summary>
    public VehicleId VehicleId => (VehicleId)Payload!;

    /// <inheritdoc />
    public VehicleRegistered(VehicleId data) : base("VehicleRegistered", data)
    {
    }
}
