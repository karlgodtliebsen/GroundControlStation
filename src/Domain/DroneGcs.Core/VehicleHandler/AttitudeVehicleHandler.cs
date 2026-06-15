using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.VehicleHandler;

/// <inheritdoc />
public sealed class AttitudeVehicleHandler(IVehicleRegistry vehicleRegistry) : IAttitudeVehicleHandler
{
    /// <inheritdoc />
    public void Handle(AttitudeMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        var vehicle = vehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyAttitude(
            message.Roll,
            message.Pitch,
            message.Yaw);
    }
}
