using DroneGcs.Core.VehicleHandler;

using DroneGs.MavLink;
using DroneGs.MavLink.Messages;

namespace DroneGcs.Core;

/// <summary>
/// Pumps messages from a vehicle connection and handles them.
/// </summary>
/// <param name="connection">The MAVLink connection to read messages from.</param>
/// <param name="heartbeatHandler">The handler for heartbeat messages.</param>
public sealed class VehicleMessagePump(IMavLinkConnection connection, IHeartbeatVehicleHandler heartbeatHandler) : IVehicleMessagePump
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in connection.ReadMessagesAsync(cancellationToken))
        {
            if (message is HeartbeatMessage heartbeat)
            {
                heartbeatHandler.Handle(heartbeat);
            }
        }
    }
}
