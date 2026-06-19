using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles status text messages from vehicles.
/// </summary>
/// <param name="vehicleRegistry">The vehicle registry to update.</param>
/// <param name="logger">The logger to use for logging.</param>
public sealed class StatusTextHandler(IVehicleRegistry vehicleRegistry, ILogger<StatusTextHandler> logger) : IStatusTextHandler
{
    /// <inheritdoc />
    public void Handle(StatusTextMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);
        logger.LogTrace("Handling status text message from vehicle {VehicleId}", vehicleId);
        var vehicle = vehicleRegistry.GetRequired(vehicleId);
        vehicle?.ApplyStatusText(message);
    }
}
