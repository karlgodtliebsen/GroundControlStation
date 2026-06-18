using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles status text messages from vehicles.
/// </summary>
/// <param name="VehicleRegistry"></param>
/// <param name="Logger"></param>
public sealed record StatusTextHandler(IVehicleRegistry VehicleRegistry, ILogger<StatusTextHandler> Logger) : IStatusTextHandler
{
    /// <inheritdoc />
    public void Handle(StatusTextMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);
        Logger.LogTrace("Handling status text message from vehicle {VehicleId}", vehicleId);
        var vehicle = VehicleRegistry.GetRequired(vehicleId);
        vehicle.ApplyStatusText(message);
    }
}
