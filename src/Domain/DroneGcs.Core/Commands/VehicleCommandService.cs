using Domain.Library.DateTime.Domain;

using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using DroneGs.MavLink.Commands;
using DroneGs.MavLink.Encoding;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.Logging;

namespace DroneGcs.Core.Commands;

/// <summary>
/// Service for sending commands to vehicles.
/// </summary>
public sealed class VehicleCommandService(
    IVehicleRegistry registry,
    IDomainEventHub eventHub,
    IMavLinkConnection connection,
    IMavLinkCommandEncoder encoder,
    ICommandAckTracker commandAckTracker,
    IDateTimeProvider dateTimeProvider,
    IVehicleCommandPolicy commandPolicy,
    ILogger<VehicleCommandService> logger)
    : IVehicleCommandService
{
    private static readonly TimeSpan CommandAckTimeout = TimeSpan.FromSeconds(5);

    private VehicleCommandResponse? ValidateCanSetMode(VehicleId vehicleId, VehicleMode mode)
    {
        var vehicle = registry.GetRequired(vehicleId);
        if (vehicle is null)
        {
            return new VehicleCommandResponse(vehicleId, VehicleCommandResult.VehicleNotFound, dateTimeProvider.UtcNow);
        }

        var validation = commandPolicy.ValidateSetMode(vehicle.State, mode);
        return validation;
    }

    private VehicleCommandResponse? ValidateCanCommand(VehicleId vehicleId)
    {
        var vehicle = registry.GetRequired(vehicleId);
        if (vehicle is null)
        {
            return new VehicleCommandResponse(vehicleId, VehicleCommandResult.VehicleNotFound, dateTimeProvider.UtcNow);
        }

        var validation = commandPolicy.ValidateArm(vehicle.State);
        return validation;
    }

    /// <inheritdoc />
    public async Task<VehicleCommandResponse> ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        var validation = ValidateCanCommand(vehicleId);
        if (validation is not null)
        {
            return validation;
        }

        var result = await SendArmDisarmAsync(vehicleId, true, cancellationToken);

        if (result.Result == VehicleCommandResult.Accepted)
        {
            await eventHub.PublishDomainEventAsync(new VehicleArmed(vehicleId), cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<VehicleCommandResponse> DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        var validation = ValidateCanCommand(vehicleId);
        if (validation is not null)
        {
            return validation;
        }

        var result = await SendArmDisarmAsync(vehicleId, false, cancellationToken);
        if (result.Result == VehicleCommandResult.Accepted)
        {
            await eventHub.PublishDomainEventAsync(new VehicleDisarmed(vehicleId), cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<VehicleCommandResponse> SetModeAsync(VehicleId vehicleId, VehicleMode mode, CancellationToken cancellationToken)
    {
        var validation = ValidateCanCommand(vehicleId);

        if (validation is not null)
        {
            return validation;
        }

        validation = ValidateCanSetMode(vehicleId, mode);

        if (validation is not null)
        {
            return validation;
        }

        var customMode = ArduCopterModeMapper.ToCustomMode(mode);

        var waitForAckTask = commandAckTracker.WaitForAckAsync(vehicleId, MavLinkCommandIds.DoSetMode, CommandAckTimeout, cancellationToken);

        var packet = encoder.EncodeSetMode(vehicleId.SystemId, vehicleId.ComponentId, customMode);

        await connection.SendRawAsync(packet, cancellationToken);

        try
        {
            var ack = await waitForAckTask.ConfigureAwait(false);
            await eventHub.PublishDomainEventAsync(new VehicleModeChanged((vehicleId, mode, dateTimeProvider.UtcNow)), cancellationToken);
            return new VehicleCommandResponse(vehicleId, MapResult(ack.Result), ack.ReceivedAt);
        }
        catch (TimeoutException)
        {
            return new VehicleCommandResponse(vehicleId, VehicleCommandResult.Timeout, dateTimeProvider.UtcNow);
        }
    }


    private async Task<VehicleCommandResponse> SendArmDisarmAsync(VehicleId vehicleId, bool arm, CancellationToken cancellationToken)
    {
        var validation = ValidateCanCommand(vehicleId);

        if (validation is not null)
        {
            return validation;
        }

        var waitForAckTask = commandAckTracker.WaitForAckAsync(vehicleId, MavLinkCommandIds.ComponentArmDisarm, CommandAckTimeout, cancellationToken);

        var packet = encoder.EncodeArmDisarm(vehicleId.SystemId, vehicleId.ComponentId, arm);

        await connection.SendRawAsync(packet, cancellationToken).ConfigureAwait(false);

        try
        {
            var ack = await waitForAckTask.ConfigureAwait(false);

            return new VehicleCommandResponse(vehicleId, MapResult(ack.Result), ack.ReceivedAt);
        }
        catch (TimeoutException)
        {
            return new VehicleCommandResponse(vehicleId, VehicleCommandResult.Timeout, dateTimeProvider.UtcNow);
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

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync().ConfigureAwait(false);
    }
}
