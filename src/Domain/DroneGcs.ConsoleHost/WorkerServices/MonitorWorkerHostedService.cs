using DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;

using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// Hosted service for monitoring worker tasks.
/// </summary>
/// <param name="workerService">The worker service to monitor.</param>
/// <param name="logger">The logger instance.</param>
public class MonitorWorkerHostedService(IMonitorWorkerService workerService, ILogger<MonitorWorkerHostedService> logger)
    : SingleTaskMainServiceHostedService(workerService, logger)
{
}
