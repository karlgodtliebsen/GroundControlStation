using System.Net;
using System.Net.Sockets;

namespace DroneGcs.Transport;

/// <inheritdoc />
public sealed class UdpMavLinkTransport : IMavLinkTransport
{
    private readonly UdpClient udpClient;
    private readonly IPEndPoint remoteEndPoint;
    private readonly MavLinkEndpoint localEndpoint;

    private volatile bool isConnected;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localPort"></param>
    /// <param name="remoteHost"></param>
    /// <param name="remotePort"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public UdpMavLinkTransport(int localPort, string remoteHost, int remotePort)
    {
        if (localPort is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(localPort));
        }

        if (remotePort is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(remotePort));
        }

        if (string.IsNullOrWhiteSpace(remoteHost))
        {
            throw new ArgumentException("Remote host must be specified.", nameof(remoteHost));
        }

        udpClient = new UdpClient(localPort);

        var remoteAddress = IPAddress.Parse(remoteHost);
        remoteEndPoint = new IPEndPoint(remoteAddress, remotePort);

        localEndpoint = new MavLinkEndpoint("udp", "0.0.0.0", localPort);
    }

    /// <inheritdoc />
    public bool IsConnected => isConnected;

    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        isConnected = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<TransportReceiveResult> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (!isConnected)
        {
            throw new InvalidOperationException("UDP transport is not connected.");
        }

        var result = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);

        var bytesToCopy = Math.Min(result.Buffer.Length, buffer.Length);

        result.Buffer.AsMemory(0, bytesToCopy).CopyTo(buffer);

        var remoteEndpoint = new MavLinkEndpoint("udp", result.RemoteEndPoint.Address.ToString(), result.RemoteEndPoint.Port);

        return new TransportReceiveResult(bytesToCopy, remoteEndpoint);
    }

    /// <inheritdoc />
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        if (!isConnected)
        {
            throw new InvalidOperationException("UDP transport is not connected.");
        }

        await udpClient.SendAsync(data, remoteEndPoint, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        isConnected = false;
        udpClient.Close();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        isConnected = false;
        udpClient.Dispose();

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
