namespace DroneGcs.Core;

/// <summary>
/// Defines the contract for a service that can send commands to a vehicle.
/// </summary>
public interface IVehicleCommandService
{
    /// <summary>
    /// Arms the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to arm.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ArmAsync(
        VehicleId vehicleId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Disarms the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to disarm.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DisarmAsync(
        VehicleId vehicleId,
        CancellationToken cancellationToken);
}

public class VehicleCommandService : IVehicleCommandService
{
    /// <inheritdoc />
    public Task ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
