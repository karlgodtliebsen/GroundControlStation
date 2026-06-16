using DroneGcs.Core.Models;

using DroneGs.MavLink.Commands;
using DroneGs.MavLink.Encoding;
using DroneGs.MavLink.Services;

namespace DroneGcs.Core.Commands;

/// <summary>
/// Service for sending commands to vehicles.
/// </summary>
public sealed class VehicleCommandService(IMavLinkConnection connection, IMavLinkCommandEncoder encoder, ICommandAckTracker commandAckTracker)
    : IVehicleCommandService
{
    private static readonly TimeSpan CommandAckTimeout = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    public async Task<VehicleCommandResponse> ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        return await SendArmDisarmAsync(vehicleId, true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VehicleCommandResponse> DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        return await SendArmDisarmAsync(vehicleId, false, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VehicleCommandResponse> SetModeAsync(VehicleId vehicleId, VehicleMode mode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<VehicleCommandResponse> SendArmDisarmAsync(
        VehicleId vehicleId,
        bool arm,
        CancellationToken cancellationToken)
    {
        var waitForAckTask = commandAckTracker.WaitForAckAsync(
            vehicleId,
            MavLinkCommandIds.ComponentArmDisarm,
            CommandAckTimeout,
            cancellationToken);

        var packet = encoder.EncodeArmDisarm(
            vehicleId.SystemId,
            vehicleId.ComponentId,
            arm);

        await connection.SendRawAsync(
                packet,
                cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var ack = await waitForAckTask.ConfigureAwait(false);

            return new VehicleCommandResponse(
                vehicleId,
                MapResult(ack.Result),
                ack.ReceivedAt);
        }
        catch (TimeoutException)
        {
            return new VehicleCommandResponse(
                vehicleId,
                VehicleCommandResult.Timeout,
                DateTimeOffset.UtcNow);
        }
    }

    private static VehicleCommandResult MapResult(byte mavResult)
    {
        return mavResult switch
        {
            0 => VehicleCommandResult.Accepted,
            1 => VehicleCommandResult.TemporarilyRejected,
            2 => VehicleCommandResult.Denied,
            3 => VehicleCommandResult.Unsupported,
            4 => VehicleCommandResult.Failed,
            var _ => VehicleCommandResult.Failed
        };
    }
}
