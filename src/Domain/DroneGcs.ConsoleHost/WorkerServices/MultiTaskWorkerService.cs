using DroneGcs.ConsoleHost.HostingLibraries.Abstractions;
using DroneGcs.Core.Services;
using DroneGs.MavLink.Services;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <inheritdoc />
public class MultiTaskWorkerService(IMavLinkConnection connection, IVehicleMessagePump messagePump, ILogger<MultiTaskWorkerService> logger)
    : IMultiTaskWorkerService
{
    /// <inheritdoc />
    public (IList<Task?> runningTaskCollection, IList<CancellationTokenSource?> cancellationTokenSourceCollection) Run(CancellationToken cancellationToken)
    {
        IList<Task?> runningTaskCollection = [];
        IList<CancellationTokenSource?> cancellationTokenSourceCollection = [];

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationTokenSourceCollection.Add(cancellationTokenSource);

        var runningTask = Task.Run(async () => { await messagePump.StartAsync(cancellationTokenSource.Token); }, cancellationTokenSource.Token);
        runningTaskCollection.Add(runningTask);

        var connectionRunningTask = Task.Run(async () => { await connection.StartAsync(cancellationTokenSource.Token); }, cancellationTokenSource.Token);
        runningTaskCollection.Add(connectionRunningTask);
        logger.LogInformation("MultiTaskWorkerService started.");

        return (runningTaskCollection, cancellationTokenSourceCollection);
    }
}
