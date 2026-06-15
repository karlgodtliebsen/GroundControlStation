using DroneGcs.Core.VehicleHandler;

using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

namespace DroneGcs.Core;

/// <summary>
/// Pumps messages from a vehicle connection and handles them.
/// </summary>
/// <param name="connection">The MAVLink connection to read messages from.</param>
/// <param name="heartbeatHandler">The handler for heartbeat messages.</param>
/// <param name="positionHandler">The handler for position messages.</param>
/// <param name="attitudeHandler">The handler for attitude messages.</param>
/// <param name="batteryHandler">The handler for battery status messages.</param>
public sealed class VehicleMessagePump(
    IMavLinkConnection connection,
    IHeartbeatVehicleHandler heartbeatHandler,
    IPositionVehicleHandler positionHandler,
    IAttitudeVehicleHandler attitudeHandler,
    IBatteryVehicleHandler batteryHandler)
    : IVehicleMessagePump
{
    /// <summary>
    /// Starts pumping messages from the vehicle connection.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in connection.ReadMessagesAsync(cancellationToken))
        {
            switch (message)
            {
                case HeartbeatMessage heartbeat:
                    heartbeatHandler.Handle(heartbeat);
                    break;

                case GlobalPositionIntMessage position:
                    positionHandler.Handle(position);
                    break;

                case AttitudeMessage attitude:
                    attitudeHandler.Handle(attitude);
                    break;

                case SysStatusMessage sysStatus:
                    batteryHandler.Handle(sysStatus);
                    break;
            }
        }
    }
}
