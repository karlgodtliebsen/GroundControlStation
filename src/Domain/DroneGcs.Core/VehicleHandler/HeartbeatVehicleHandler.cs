using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles heartbeat messages and updates the vehicle registry accordingly.
/// </summary>
public sealed class HeartbeatVehicleHandler(IVehicleRegistry vehicleRegistry, ILogger<HeartbeatVehicleHandler> logger) : IHeartbeatVehicleHandler
{
    /// <inheritdoc />
    public VehicleSession Handle(HeartbeatMessage message)
    {
        logger.LogTrace("HeartbeatVehicleHandler - Handling heartbeat message from vehicle {VehicleId}", new VehicleId(message.SystemId, message.ComponentId));
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        var result = vehicleRegistry.RegisterOrUpdateHeartbeat(
            vehicleId,
            message.IPEndPoint,
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
