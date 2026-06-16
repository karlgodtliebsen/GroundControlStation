using DroneGcs.Core.Models;

namespace DroneGcs.Core.Commands;

/// <summary>
/// Represents the result of a vehicle command. 
/// </summary>
public enum VehicleCommandResult
{
    Accepted = 0,
    TemporarilyRejected = 1,
    Denied = 2,
    Unsupported = 3,
    Failed = 4,
    Timeout = 100
}

/// <summary>
/// Represents the response of a vehicle command.
/// </summary>
/// <param name="VehicleId">The ID of the vehicle.</param>
/// <param name="Result">The result of the command.</param>
/// <param name="CompletedAt">The timestamp when the command was completed.</param>
public sealed record VehicleCommandResponse(
    VehicleId VehicleId,
    VehicleCommandResult Result,
    DateTimeOffset CompletedAt);
