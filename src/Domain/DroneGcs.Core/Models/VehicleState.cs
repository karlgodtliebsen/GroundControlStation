namespace DroneGcs.Core.Models;

/// <summary>
/// Represents the state of a vehicle.
/// </summary>
/// <param name="VehicleId">The unique identifier of the vehicle.</param>
/// <param name="CustomMode">The custom mode of the vehicle.</param>
/// <param name="VehicleType">The type of the vehicle.</param>
/// <param name="Autopilot">The autopilot type of the vehicle.</param>
/// <param name="BaseMode">The base mode of the vehicle.</param>
/// <param name="SystemStatus">The system status of the vehicle.</param>
/// <param name="MavLinkVersion">The MAVLink version of the vehicle.</param>
/// <param name="ConnectionState">The connection state of the vehicle.</param>
/// <param name="LastHeartbeatAt">The timestamp of the last heartbeat received from the vehicle.</param>
/// <param name="Mode"></param>
/// <param name="IsArmed"></param>
public sealed record VehicleState(
    VehicleId VehicleId,
    uint CustomMode,
    byte VehicleType,
    byte Autopilot,
    byte BaseMode,
    byte SystemStatus,
    byte MavLinkVersion,
    VehicleConnectionState ConnectionState,
    DateTimeOffset LastHeartbeatAt,
    VehicleMode Mode,
    bool IsArmed);
