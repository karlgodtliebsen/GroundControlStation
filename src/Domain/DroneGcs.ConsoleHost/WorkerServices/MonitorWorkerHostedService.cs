using DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;

using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

public class MonitorWorkerHostedService(IMonitorWorkerService workerService, ILogger<MonitorWorkerHostedService> logger)
    : SingleTaskMainServiceHostedService(workerService, logger)
{
}
