namespace DroneGcs.Core;

/// <summary>
/// Represents the connection state of a vehicle.
/// </summary>
public enum VehicleConnectionState
{
    Unknown = 0,
    Online = 1,
    Stale = 2,
    Offline = 3
}
