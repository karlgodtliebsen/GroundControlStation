namespace DroneGcs.Core;

public interface IVehicleMessagePump
{
    Task StartAsync(CancellationToken cancellationToken);
}
