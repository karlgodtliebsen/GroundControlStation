namespace DroneGcs.Core;

public readonly record struct VehicleId(
    byte SystemId,
    byte ComponentId);

public sealed class Vehicle
{
    public VehicleId Id { get; }
    //public VehicleState State { get; }
}


//public interface IVehicleConnection
//{
//    VehicleId VehicleId { get; }

//    Task ArmAsync(
//        CancellationToken cancellationToken);

//    Task DisarmAsync(
//        CancellationToken cancellationToken);

//    Task SetModeAsync(
//        VehicleMode mode,
//        CancellationToken cancellationToken);

//    Task<VehicleState> GetStateAsync();
//}
