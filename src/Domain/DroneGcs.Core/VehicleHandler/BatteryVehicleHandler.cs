using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <inheritdoc />
public sealed record BatteryVehicleHandler(IVehicleRegistry VehicleRegistry, ILogger<BatteryVehicleHandler> Logger) : IBatteryVehicleHandler
{
    /// <inheritdoc />
    public void Handle(SysStatusMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        Logger.LogTrace("Handling battery status message from vehicle {VehicleId}", vehicleId);
        var vehicle = VehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyBattery(message.BatteryRemaining, message.BatteryVoltage);
    }
}
