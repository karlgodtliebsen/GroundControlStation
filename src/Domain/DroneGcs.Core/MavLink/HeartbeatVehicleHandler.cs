using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.MavLink;

/// <summary>
/// Handles heartbeat messages and updates the vehicle registry accordingly.
/// </summary>
public sealed class HeartbeatVehicleHandler : IHeartbeatVehicleHandler
{
    private readonly IVehicleRegistry vehicleRegistry;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vehicleRegistry"></param>
    public HeartbeatVehicleHandler(IVehicleRegistry vehicleRegistry)
    {
        this.vehicleRegistry = vehicleRegistry;
    }

    /// <inheritdoc />
    public VehicleSession Handle(HeartbeatMessage message)
    {
        var vehicleId = new VehicleId(
            message.SystemId,
            message.ComponentId);

        return vehicleRegistry.RegisterOrUpdateHeartbeat(
            vehicleId,
            message.CustomMode,
            message.VehicleType,
            message.Autopilot,
            message.BaseMode,
            message.SystemStatus,
            message.MavLinkVersion,
            message.ReceivedAt);
    }
}
