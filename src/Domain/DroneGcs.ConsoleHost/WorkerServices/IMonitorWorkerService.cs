using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// A worker service that monitors the system.
/// </summary>
public interface IMonitorWorkerService : ISingleTaskWorkerService
{
}
