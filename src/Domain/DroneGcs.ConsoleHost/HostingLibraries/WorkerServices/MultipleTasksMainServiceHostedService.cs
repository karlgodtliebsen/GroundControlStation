using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;

/// <summary>
/// A hosted service that manages multiple tasks and their cancellation tokens using an instance of <see cref="IMultiTaskWorkerService"/>.
/// </summary>
/// <param name="workerService"></param>
/// <param name="logger"></param>
public class MultipleTasksMainServiceHostedService(IMultiTaskWorkerService workerService, ILogger logger) : BackgroundService
{
    private IList<Task?> runningTaskCollection = [];
    private IList<CancellationTokenSource?> cancellationTokenSourceCollection = [];

    /// <summary>
    /// Executes the main logic of the service by running the worker service and managing the tasks and cancellation tokens. 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MultipleTasks Main Service:{service} with Worker: {worker} is running.", nameof(MultipleTasksMainServiceHostedService), workerService.GetType().FullName);
        var (tasks, cancellationTokenSources) = workerService.Run(cancellationToken);
        runningTaskCollection = tasks;
        cancellationTokenSourceCollection = cancellationTokenSources;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the resources used by the service.
    /// </summary>
    public override void Dispose()
    {
        logger.LogInformation("Service:{service} with Worker: {worker} is Disposed.", nameof(MultipleTasksMainServiceHostedService), workerService.GetType().FullName);
        foreach (var cancellationTokenSource in cancellationTokenSourceCollection) cancellationTokenSource?.Dispose();

        foreach (var runningTask in runningTaskCollection) runningTask?.Dispose();

        base.Dispose();
    }
}
