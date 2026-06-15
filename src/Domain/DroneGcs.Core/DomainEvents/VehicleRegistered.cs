using Domain.Library.EventHub.Events;

using DroneGcs.Core.Models;

namespace DroneGcs.Core.DomainEvents;

/// <summary>
/// Event that is triggered when a vehicle is registered.
/// </summary>
public class VehicleRegistered : DomainEvent<VehicleId>
{
    /// <inheritdoc />
    public VehicleRegistered(VehicleId data) : base("VehicleRegistered", data)
    {
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public VehicleRegistered(VehicleId data, MetaData md) : base("VehicleRegistered", data, md)
    {
    }
}

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

/// <summary>
/// Event that is triggered when a vehicle's connection state changes.
/// </summary>
public class VehicleConnectionStateChanged : DomainEvent<(VehicleId VehicleId, VehicleConnectionState ConnectionState)>
{
    /// <inheritdoc />
    public VehicleConnectionStateChanged((VehicleId VehicleId, VehicleConnectionState ConnectionState) data) : base("VehicleConnectionStateChanged", data)
    {
    }

    /// <inheritdoc />
    public VehicleConnectionStateChanged((VehicleId VehicleId, VehicleConnectionState ConnectionState) data, MetaData md) : base("VehicleConnectionStateChanged", data, md)
    {
    }
}
