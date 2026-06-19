using DroneGcs.ConsoleHost.Dashboard;
using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// A worker service that can run a single task.
/// </summary>
public class MonitorWorkerService(IVehicleConnectionMonitor monitor, MonitorOutputBuffer monitorOutputBuffer, IDomainEventHub eventHub, ILogger<MonitorWorkerService> logger) : IMonitorWorkerService
{
    /// <inheritdoc />
    public async Task Run(CancellationToken stoppingToken)
    {
        logger.LogInformation("Console command service started.");
        eventHub.SubscribeDomainEvent<VehicleArmed>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleDisarmed>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleConnectionStateChanged>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleModeChanged>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleRegistered>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleStateUpdated>((e) => monitorOutputBuffer.Add(e.Name));
        eventHub.SubscribeDomainEvent<VehicleStatusMessageReceived>((e) => monitorOutputBuffer.Add(e.Name));

        //start a scheduled task to update the monitor output buffer every second
        _ = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                monitor.UpdateConnectionStates();
                //monitorOutputBuffer.Update();
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }, stoppingToken);
    }
}
