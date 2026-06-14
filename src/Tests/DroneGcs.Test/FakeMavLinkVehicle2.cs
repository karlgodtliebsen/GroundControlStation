using System.Net;
using System.Net.Sockets;

using DroneGs.MavLink;

namespace DroneGcs.Test;

/// <summary>
/// A simple MAVLink vehicle simulator that sends fake MAVLink messages over UDP.
/// </summary>
public sealed class FakeMavLinkVehicle2 : IAsyncDisposable
{
    private readonly UdpClient udpClient;
    private readonly IPEndPoint targetEndpoint;
    private readonly TimeSpan heartbeatInterval;

    private CancellationTokenSource? cancellationTokenSource;
    private Task? workerTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeMavLinkVehicle2"/> class.
    /// </summary>
    /// <param name="targetIp">The IP address of the target endpoint.</param>
    /// <param name="targetPort">The port of the target endpoint.</param>
    /// <param name="heartbeatInterval">The interval at which heartbeat messages are sent.</param>
    public FakeMavLinkVehicle2(string targetIp, int targetPort, TimeSpan? heartbeatInterval = null)
    {
        udpClient = new UdpClient();

        targetEndpoint = new IPEndPoint(IPAddress.Parse(targetIp), targetPort);

        this.heartbeatInterval = heartbeatInterval ?? TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        workerTask = Task.Run(() => SendLoopAsync(cancellationTokenSource.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var heartbeat = MavLinkKnownFrames.CreateHeartbeatV2();

            await udpClient.SendAsync(heartbeat, targetEndpoint, cancellationToken).ConfigureAwait(false);

            await Task.Delay(heartbeatInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="FakeMavLinkVehicle2"/> instance.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync().ConfigureAwait(false);

            if (workerTask is not null)
            {
                try
                {
                    await workerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }

            cancellationTokenSource.Dispose();
        }

        udpClient.Dispose();
    }
}
