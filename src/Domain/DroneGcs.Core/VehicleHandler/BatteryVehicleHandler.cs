using DroneGcs.Core.Models;
using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.VehicleHandler;

/// <inheritdoc />
public sealed record BatteryVehicleHandler(IVehicleRegistry vehicleRegistry) : IBatteryVehicleHandler
{
    /// <inheritdoc />
    public void Handle(SysStatusMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        var vehicle = vehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyBattery(
            message.BatteryRemaining,
            message.BatteryVoltage);
    }
}
