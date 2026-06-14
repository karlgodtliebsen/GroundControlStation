using DroneGcs.Core.MavLink;
using DroneGs.MavLink;
using DroneGs.MavLink.Messages;

namespace DroneGcs.Core;

public sealed class VehicleMessagePump(
    IMavLinkConnection connection,
    IHeartbeatVehicleHandler heartbeatHandler)
    : IVehicleMessagePump
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in connection.ReadMessagesAsync(cancellationToken))
            if (message is HeartbeatMessage heartbeat)
                heartbeatHandler.Handle(heartbeat);
    }
}
