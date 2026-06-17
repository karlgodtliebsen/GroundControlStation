using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles position messages and updates the vehicle registry accordingly.
/// </summary>
/// <param name="vehicleRegistry">The vehicle registry to update.</param>
/// <param name="logger"></param>
public sealed class PositionVehicleHandler(IVehicleRegistry vehicleRegistry, ILogger<PositionVehicleHandler> logger) : IPositionVehicleHandler
{
    /// <inheritdoc/>
    public void Handle(GlobalPositionIntMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        logger.LogDebug("Handling position message from vehicle {VehicleId}", vehicleId);
        var vehicle = vehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyPosition(message.Latitude, message.Longitude, message.Altitude);
    }
}
