using DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

/// <summary>
/// A hosted service that manages a single task.
/// </summary>
/// <param name="workerService">The worker service that provides the task to run.</param>
/// <param name="logger">The logger instance for logging.</param>
public class SingleTaskHostedService(ISingleTaskWorkerService workerService, ILogger<SingleTaskHostedService> logger)
    : SingleTaskMainServiceHostedService(workerService, logger)
{
}
