namespace DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

/// <summary>
/// Defines a contract for a worker service that can run a single task.
/// </summary>
public interface ISingleTaskWorkerService
{
    /// <summary>
    /// Runs a single task.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Run(CancellationToken cancellationToken);
}
