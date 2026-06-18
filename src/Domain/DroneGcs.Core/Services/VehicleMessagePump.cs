using Domain.Library.EventHub.Abstractions;

using DroneGcs.Core.Commands;
using DroneGcs.Core.VehicleHandler;

using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.Services;

/// <summary>
/// Pumps messages from a vehicle connection and handles them.
/// </summary>
/// <param name="heartbeatHandler">The handler for heartbeat messages.</param>
/// <param name="positionHandler">The handler for position messages.</param>
/// <param name="attitudeHandler">The handler for attitude messages.</param>
/// <param name="batteryHandler">The handler for battery status messages.</param>
/// <param name="commandAckTracker"></param>
/// <param name="eventHub"></param>
/// <param name="logger"></param>
public sealed class VehicleMessagePump(
    IHeartbeatVehicleHandler heartbeatHandler,
    IPositionVehicleHandler positionHandler,
    IAttitudeVehicleHandler attitudeHandler,
    IBatteryVehicleHandler batteryHandler,
    ICommandAckTracker commandAckTracker,
    IEventHub eventHub,
    ILogger<VehicleMessagePump> logger)
    : IVehicleMessagePump
{
    private IDisposable? subscription;

    /// <summary>
    /// Starts pumping messages from the vehicle connection.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("VehicleMessagePump - Starting Event Subscription.");
        subscription = eventHub.SubscribeAsync<MavLinkMessage>(MavLinkEventTopics.ReceivedMessage, HandleMessage);
    }

    private async Task HandleMessage(MavLinkMessage message, CancellationToken cancellationToken)
    {
        logger.LogTrace("VehicleMessagePump - Received {MessageType}", message.GetType().Name);
        switch (message)
        {
            case HeartbeatMessage heartbeat:
                logger.LogInformation("VehicleMessagePump - Handling heartbeat from {SystemId}:{ComponentId}", heartbeat.SystemId, heartbeat.ComponentId);
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

            case CommandAckMessage commandAck:
                commandAckTracker.Handle(commandAck);
                break;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        subscription?.Dispose();
        return ValueTask.CompletedTask;
    }
}
