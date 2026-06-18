using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.HostingLibraries.WorkerServices;

/// <summary>
/// A hosted service that runs a single task worker service with retry policy support.
/// </summary>
/// <param name="workerService">The worker service to be executed.</param>
/// <param name="logger">The logger instance for logging.</param>
public abstract class SingleTaskMainServiceHostedService(ISingleTaskWorkerService workerService, ILogger logger) : BackgroundService
{
    private Task? runningTask;

    private const int ContinuousRetryIntervalMinutes = 1;
    private readonly TimeSpan continuousRetryTimeSpan = TimeSpan.FromMinutes(ContinuousRetryIntervalMinutes);

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serviceName = workerService.GetType().FullName ?? "";
        if (logger.IsEnabled(LogLevel.Trace)) logger.LogTrace("SingleTask Main Service:{service} with Worker: {worker} is running.", nameof(SingleTaskMainServiceHostedService), serviceName);

        var combinedPolicy = HostingPolicyBuilder.CreateRetryPolicy(serviceName, continuousRetryTimeSpan, logger);

        runningTask = combinedPolicy.ExecuteAsync(async (ct) =>
        {
            if (!ct.IsCancellationRequested) await workerService.Run(cancellationToken);
        }, cancellationToken);

        return runningTask;
    }


    /// <summary>
    /// Disposes the hosted service, ensuring that any running tasks are properly cleaned up.
    /// </summary>
    public override void Dispose()
    {
        if (logger.IsEnabled(LogLevel.Trace)) logger.LogTrace("Service:{service} with Worker: {worker} is Disposed.", nameof(SingleTaskMainServiceHostedService), workerService.GetType().FullName);

        if (runningTask is not null)
        {
            if (runningTask.IsCompleted) runningTask.Dispose();

            runningTask = null;
        }

        base.Dispose();
    }
}
