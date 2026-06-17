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
