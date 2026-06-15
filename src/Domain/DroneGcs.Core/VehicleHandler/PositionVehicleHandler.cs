using DroneGcs.Core.Models;

using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles position messages and updates the vehicle registry accordingly.
/// </summary>
/// <param name="vehicleRegistry">The vehicle registry to update.</param>
public sealed class PositionVehicleHandler(IVehicleRegistry vehicleRegistry) : IPositionVehicleHandler
{
    /// <inheritdoc/>
    public void Handle(GlobalPositionIntMessage message)
    {
        var vehicleId = new VehicleId(message.SystemId, message.ComponentId);

        var vehicle = vehicleRegistry.GetRequired(vehicleId);

        vehicle.ApplyPosition(message.Latitude, message.Longitude, message.Altitude);
    }
}
