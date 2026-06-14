using Domain.Library.DateTime.Domain;

namespace DroneGcs.Core;

public sealed class VehicleConnectionMonitor(
    IVehicleRegistry vehicleRegistry,
    IDateTimeProvider clock)
    : IVehicleConnectionMonitor
{
    private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan OfflineAfter = TimeSpan.FromSeconds(5);

    public void UpdateConnectionStates()
    {
        vehicleRegistry.UpdateConnectionStates(clock.UtcNow, StaleAfter, OfflineAfter);
    }
}
