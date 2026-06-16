using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Options;

namespace DroneGcs.Transport;

/// <inheritdoc />
public sealed class UdpMavLinkTransport : IMavLinkTransport
{
    private readonly UdpClient udpClient;
    private readonly IPEndPoint remoteEndPoint;
    private readonly MavLinkEndpoint localEndpoint;
    private readonly TransportEndpoint endpoint;
    private volatile bool isConnected;

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public UdpMavLinkTransport(IOptions<TransportEndpoint> options)
    {
        endpoint = options.Value;

        var localPort = endpoint.LocalPort;
        var remoteHost = endpoint.RemoteHost;
        var remotePort = endpoint.RemotePort;
        var localHost = endpoint.LocalHost;

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

        var localAddress = string.IsNullOrWhiteSpace(localHost)
            ? IPAddress.Any
            : IPAddress.Parse(localHost);

        udpClient = new UdpClient(
            new IPEndPoint(localAddress, localPort));

        var remoteAddress = IPAddress.Parse(remoteHost);
        remoteEndPoint = new IPEndPoint(remoteAddress, remotePort);
        localEndpoint = new MavLinkEndpoint(endpoint.Protocol, localHost, localPort);
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

        var remoteEndpoint = new MavLinkEndpoint(endpoint.Protocol, result.RemoteEndPoint.Address.ToString(), result.RemoteEndPoint.Port);

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
