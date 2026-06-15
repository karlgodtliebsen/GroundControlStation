using DroneGcs.Core.Models;

namespace DroneGcs.Core.Commands;

/// <summary>
/// 
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

public sealed record VehicleCommandResponse(
    VehicleId VehicleId,
    VehicleCommandResult Result,
    DateTimeOffset CompletedAt);
