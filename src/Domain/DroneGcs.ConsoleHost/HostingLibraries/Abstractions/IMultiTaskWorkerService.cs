namespace DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

/// <summary>
/// Defines a contract for a worker service that can run multiple tasks concurrently.
/// </summary>
public interface IMultiTaskWorkerService
{
    /// <summary>
    /// Runs multiple tasks concurrently.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>A tuple containing the collection of running tasks and their corresponding cancellation token sources.</returns>
    (IList<Task?> runningTaskCollection, IList<CancellationTokenSource?> cancellationTokenSourceCollection) Run(CancellationToken cancellationToken);
}
