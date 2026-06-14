namespace DroneGcs.Core;

/// <summary>
/// Manages the registration and state of vehicles.
/// </summary>
public sealed class VehicleRegistry : IVehicleRegistry
{
    private readonly Dictionary<VehicleId, VehicleSession> vehicles = [];

    /// <summary>
    /// Gets the collection of registered vehicle sessions.
    /// </summary>
    public IReadOnlyCollection<VehicleSession> Vehicles => vehicles.Values.ToArray();

    /// <inheritdoc />
    public void UpdateConnectionStates(DateTimeOffset now, TimeSpan staleAfter, TimeSpan offlineAfter)
    {
        foreach (var vehicle in vehicles.Values)
        {
            vehicle.UpdateConnectionState(now, staleAfter, offlineAfter);
        }
    }

    /// <summary>
    /// Registers a new vehicle or updates an existing vehicle's state based on a received heartbeat message. 
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <param name="customMode"></param>
    /// <param name="vehicleType"></param>
    /// <param name="autopilot"></param>
    /// <param name="baseMode"></param>
    /// <param name="systemStatus"></param>
    /// <param name="mavLinkVersion"></param>
    /// <param name="receivedAt"></param>
    /// <returns></returns>
    public VehicleSession RegisterOrUpdateHeartbeat(
        VehicleId vehicleId,
        uint customMode,
        byte vehicleType,
        byte autopilot,
        byte baseMode,
        byte systemStatus,
        byte mavLinkVersion,
        DateTimeOffset receivedAt)
    {
        if (!vehicles.TryGetValue(vehicleId, out var session))
        {
            var state = new VehicleState(
                vehicleId,
                customMode,
                vehicleType,
                autopilot,
                baseMode,
                systemStatus,
                mavLinkVersion,
                VehicleConnectionState.Online,
                receivedAt);

            session = new VehicleSession(state);
            vehicles.Add(vehicleId, session);

            return session;
        }

        session.ApplyHeartbeat(
            customMode,
            vehicleType,
            autopilot,
            baseMode,
            systemStatus,
            mavLinkVersion,
            receivedAt);

        return session;
    }
}
