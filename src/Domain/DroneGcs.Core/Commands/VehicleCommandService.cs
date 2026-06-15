namespace DroneGcs.Core.Commands;

public class VehicleCommandService : IVehicleCommandService
{
    /// <inheritdoc />
    public Task<VehicleCommandResponse> ArmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<VehicleCommandResponse> DisarmAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
