using DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// A hosted service that manages a single task.
/// </summary>
/// <param name="workerService">The worker service that provides the task to run.</param>
/// <param name="logger">The logger instance for logging.</param>
public class CommandWorkerHostedService(ICommandWorkerService workerService, ILogger<CommandWorkerHostedService> logger)
    : SingleTaskMainServiceHostedService(workerService, logger)
{
}
