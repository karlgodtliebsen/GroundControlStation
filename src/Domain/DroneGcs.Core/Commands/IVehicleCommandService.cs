using DroneGcs.Core.Models;

namespace DroneGcs.Core.Commands;

/// <summary>
/// Defines the contract for a service that can send commands to a vehicle.
/// </summary>
public interface IVehicleCommandService : IAsyncDisposable
{
    /// <summary>
    /// Arms the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to arm.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<VehicleCommandResponse> ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken);


    /// <summary>
    /// Disarms the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to disarm.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<VehicleCommandResponse> DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <param name="mode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<VehicleCommandResponse> SetModeAsync(VehicleId vehicleId, VehicleMode mode, CancellationToken cancellationToken);
}
