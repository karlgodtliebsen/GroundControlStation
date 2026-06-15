using Domain.Library.EventHub.Events;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle's state is updated.
/// </summary>
public class VehicleStateUpdated : DomainEvent<VehicleState>
{
    /// <inheritdoc />
    public VehicleStateUpdated(VehicleState data) : base("VehicleStateUpdated", data)
    {
    }

    /// <inheritdoc />
    public VehicleStateUpdated(VehicleState data, MetaData md) : base("VehicleStateUpdated", data, md)
    {
    }
}
