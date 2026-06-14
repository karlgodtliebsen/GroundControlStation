namespace DroneGcs.Core;

/// <summary>
/// Represents a session for a vehicle, managing its state and handling updates.
/// </summary>
public sealed class VehicleSession(VehicleState initialState)
{
    private VehicleState state = initialState;

    /// <summary>
    /// Gets the unique identifier of the vehicle.
    /// </summary>
    public VehicleId Id => state.VehicleId;

    /// <summary>
    /// Gets the current state of the vehicle.
    /// </summary>
    public VehicleState State => state;

    /// <summary>
    /// Updates the connection state of the vehicle based on the elapsed time since the last heartbeat.
    /// </summary>
    /// <param name="now">The current date and time.</param>
    /// <param name="staleAfter">The time span after which the vehicle is considered stale.</param>
    /// <param name="offlineAfter">The time span after which the vehicle is considered offline.</param>
    public void UpdateConnectionState(
        DateTimeOffset now,
        TimeSpan staleAfter,
        TimeSpan offlineAfter)
    {
        var age = now - state.LastHeartbeatAt;

        var connectionState =
            age > offlineAfter
                ? VehicleConnectionState.Offline
                : age > staleAfter
                    ? VehicleConnectionState.Stale
                    : VehicleConnectionState.Online;

        state = state with
        {
            ConnectionState = connectionState
        };
    }


    /// <summary>
    /// Applies a heartbeat message to update the state of the vehicle.
    /// </summary>
    /// <param name="customMode">The custom mode of the vehicle.</param>
    /// <param name="vehicleType">The type of the vehicle.</param>
    /// <param name="autopilot">The autopilot type of the vehicle.</param>
    /// <param name="baseMode">The base mode of the vehicle.</param>
    /// <param name="systemStatus">The system status of the vehicle.</param>
    /// <param name="mavLinkVersion">The MAVLink version of the vehicle.</param>
    /// <param name="receivedAt">The timestamp when the heartbeat was received.</param>
    public void ApplyHeartbeat(
        uint customMode,
        byte vehicleType,
        byte autopilot,
        byte baseMode,
        byte systemStatus,
        byte mavLinkVersion,
        DateTimeOffset receivedAt)
    {
        state = state with
        {
            CustomMode = customMode,
            VehicleType = vehicleType,
            Autopilot = autopilot,
            BaseMode = baseMode,
            SystemStatus = systemStatus,
            MavLinkVersion = mavLinkVersion,
            ConnectionState = VehicleConnectionState.Online,
            LastHeartbeatAt = receivedAt
        };
    }
}
