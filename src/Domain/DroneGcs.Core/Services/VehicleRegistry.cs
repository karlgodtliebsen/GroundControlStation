using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Models;


namespace DroneGcs.Core.Services;

/// <summary>
/// Manages the registration and state of vehicles.
/// </summary>
public sealed class VehicleRegistry(IDomainEventHub eventHub) : IVehicleRegistry
{
    private readonly Dictionary<VehicleId, VehicleSession> vehicles = [];

    /// <inheritdoc />
    public VehicleSession GetRequired(VehicleId vehicleId)
    {
        return !vehicles.TryGetValue(vehicleId, out var vehicle)
            ? throw new InvalidOperationException(
                $"Vehicle '{vehicleId}' is not registered.")
            : vehicle;
    }

    /// <summary>
    /// Gets the collection of registered vehicle sessions.
    /// </summary>
    public IReadOnlyCollection<VehicleSession> Vehicles => vehicles.Values.ToArray();

    /// <inheritdoc />
    public VehicleUpdateConnectionStateResult UpdateConnectionStates(DateTimeOffset now, TimeSpan staleAfter, TimeSpan offlineAfter)
    {
        var result = new List<VehicleSession>();
        foreach (var vehicle in vehicles.Values)
        {
            result.Add(vehicle);
            var stateChanged = vehicle.UpdateConnectionState(now, staleAfter, offlineAfter);
            eventHub.PublishDomainEvent(new VehicleStateUpdated(vehicle.State));
            if (stateChanged != null)
            {
                eventHub.PublishDomainEvent(stateChanged);
            }
        }

        return new VehicleUpdateConnectionStateResult(result);
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
    /// <param name="receivedAt">The timestamp when the heartbeat was received.</param>
    /// <returns>The updated or newly registered vehicle session.</returns>
    public VehicleRegistryResult RegisterOrUpdateHeartbeat(
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
                VehicleConnectionState.Unknown,
                receivedAt,
                VehicleMode.Unknown,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);


            session = new VehicleSession(state);
            vehicles.Add(vehicleId, session);
        }

        session.ApplyHeartbeat(
            customMode,
            vehicleType,
            autopilot,
            baseMode,
            systemStatus,
            mavLinkVersion,
            receivedAt);


        eventHub.PublishDomainEvent(new VehicleRegistered(vehicleId));
        return new VehicleRegistryResult(session);
    }
}
