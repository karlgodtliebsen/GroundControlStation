using DroneGcs.Core.Commands;
using DroneGcs.Core.Models;

namespace DroneGcs.Core.Services;

/// <summary>
/// 
/// </summary>
/// <param name="registry"></param>
/// <param name="commandService"></param>
public sealed class VehicleService(IVehicleRegistry registry, IVehicleCommandService commandService) : IVehicleService
{
    /// <inheritdoc />
    public IReadOnlyCollection<VehicleState> GetVehicles()
    {
        return registry.Vehicles
            .Select(vehicle => vehicle.State)
            .ToArray();
    }

    /// <inheritdoc />
    public VehicleState GetVehicle(VehicleId vehicleId)
    {
        return registry.GetRequired(vehicleId).State;
    }

    /// <inheritdoc />
    public Task<VehicleCommandResponse> ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        EnsureVehicleExists(vehicleId);

        return commandService.ArmAsync(vehicleId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VehicleCommandResponse> DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        EnsureVehicleExists(vehicleId);

        return commandService.DisarmAsync(vehicleId, cancellationToken);
    }

    /// <inheritdoc />  
    public Task<VehicleCommandResponse> SetModeAsync(VehicleId vehicleId, VehicleMode mode, CancellationToken cancellationToken)
    {
        EnsureVehicleExists(vehicleId);

        return commandService.SetModeAsync(vehicleId, mode, cancellationToken);
    }

    private void EnsureVehicleExists(VehicleId vehicleId)
    {
        registry.GetRequired(vehicleId);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await commandService.DisposeAsync().ConfigureAwait(false);
    }
}
