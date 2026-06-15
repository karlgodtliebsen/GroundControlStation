using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.VehicleHandler;

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
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        var result = vehicleRegistry.RegisterOrUpdateHeartbeat(
            vehicleId,
            message.CustomMode,
            message.VehicleType,
            message.Autopilot,
            message.BaseMode,
            message.SystemStatus,
            message.MavLinkVersion,
            message.ReceivedAt);

        return result.Vehicle;
    }
}
