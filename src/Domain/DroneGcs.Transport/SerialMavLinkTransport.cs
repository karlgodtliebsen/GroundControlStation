using System.IO.Ports;

namespace DroneGcs.Transport;

/// <summary>
/// Represents a MAVLink transport that communicates over a serial port.
/// </summary>
public sealed class SerialMavLinkTransport : IMavLinkTransport
{
    private readonly SerialPort serialPort;
    private readonly MavLinkEndpoint endpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialMavLinkTransport"/> class.
    /// </summary>
    /// <param name="portName">The name of the serial port.</param>
    /// <param name="baudRate">The baud rate for the serial port.</param>
    /// <exception cref="ArgumentException">Thrown when the port name is null or whitespace.</exception>
    public SerialMavLinkTransport(string portName, int baudRate = 115200)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            throw new ArgumentException("Port name must be specified.", nameof(portName));
        }

        serialPort = new SerialPort(
            portName,
            baudRate,
            Parity.None,
            8,
            StopBits.One)
        {
            ReadTimeout = Timeout.Infinite,
            WriteTimeout = Timeout.Infinite
        };

        endpoint = new MavLinkEndpoint("serial", portName);
    }

    /// <inheritdoc />
    public bool IsConnected => serialPort.IsOpen;

    /// <inheritdoc />
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!serialPort.IsOpen)
        {
            serialPort.Open();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<TransportReceiveResult> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Serial port is closed.");
        }

        var bytesRead = await serialPort.BaseStream
            .ReadAsync(buffer, cancellationToken)
            .ConfigureAwait(false);

        return new TransportReceiveResult(bytesRead, endpoint);
    }

    /// <inheritdoc />
    public async ValueTask WriteAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Serial port is closed.");
        }

        await serialPort.BaseStream
            .WriteAsync(data, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        serialPort.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
