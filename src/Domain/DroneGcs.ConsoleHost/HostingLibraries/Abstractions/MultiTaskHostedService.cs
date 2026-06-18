using DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

/// <summary>
/// A hosted service that manages multiple tasks.
/// </summary>
/// <param name="workerService">The worker service that provides the tasks to run.</param>
/// <param name="logger">The logger instance for logging.</param>
public class MultiTaskHostedService(IMultiTaskWorkerService workerService, ILogger<MultiTaskHostedService> logger)
    : MultipleTasksMainServiceHostedService(workerService, logger)
{
}
