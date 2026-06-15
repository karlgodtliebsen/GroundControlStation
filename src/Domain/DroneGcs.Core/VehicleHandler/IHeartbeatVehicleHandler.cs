using DroneGcs.Core.Services;

using DroneGs.MavLink.Messages;

namespace DroneGcs.Core.VehicleHandler;

/// <summary>
/// Handles heartbeat messages and updates the vehicle registry accordingly.
/// </summary>
public interface IHeartbeatVehicleHandler
{
    /// <summary>
    /// Handles a heartbeat message and updates the vehicle registry accordingly.
    /// </summary>
    /// <param name="message">The heartbeat message to handle.</param>
    /// <returns>The updated vehicle session.</returns>
    VehicleSession Handle(HeartbeatMessage message);
}
