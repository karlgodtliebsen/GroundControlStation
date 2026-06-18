using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <inheritdoc />
public sealed class AttitudeVehicleHandler(IVehicleRegistry vehicleRegistry, ILogger<AttitudeVehicleHandler> logger) : IAttitudeVehicleHandler
{
    /// <inheritdoc />
    public void Handle(AttitudeMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        logger.LogTrace("Handling attitude message from vehicle {VehicleId}", vehicleId);
        var vehicle = vehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyAttitude(
            message.Roll,
            message.Pitch,
            message.Yaw);
    }
}
