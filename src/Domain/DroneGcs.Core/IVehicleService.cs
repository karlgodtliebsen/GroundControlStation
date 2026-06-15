using DroneGcs.Core.Models;

namespace DroneGcs.Core;

/// <summary>
/// Defines the contract for a vehicle service that provides operations for managing and controlling vehicles.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Gets the list of all registered vehicles.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An array of <see cref="VehicleId"/> representing the registered vehicles.</returns>
    Task<VehicleId[]> GetVehiclesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current state of a specific vehicle.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The current state of the vehicle.</returns>
    Task<VehicleState> GetStateAsync(VehicleId vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Arms a specific vehicle.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Disarms a specific vehicle.
    /// </summary>
    /// <param name="vehicleId">The unique identifier of the vehicle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken);
}
