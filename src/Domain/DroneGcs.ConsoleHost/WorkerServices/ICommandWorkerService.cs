using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// A worker service that can run a single task.
/// </summary>
public interface ICommandWorkerService : ISingleTaskWorkerService
{
}
