using DroneGs.MavLink.Messages;

namespace DroneGs.MavLink;

//Frame parser
//Frame serializer
//Message definitions
//CRC handling
//Dialect generation

/// <summary>
/// Represents a connection to a MAVLink device.
/// </summary>
public interface IMavLinkConnection : IAsyncDisposable
{
    /// <summary>
    /// Starts the MAVLink connection, allowing it to receive and process incoming data.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads MAVLink frames from the connected device.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of MAVLink frames.</returns>
    IAsyncEnumerable<MavLinkFrame> ReadFramesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads MAVLink messages from the connected device.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of MAVLink messages.</returns>
    IAsyncEnumerable<MavLinkMessage> ReadMessagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends a MAVLink message to the connected device.
    /// </summary>
    /// <param name="message">The MAVLink message to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendAsync(MavLinkMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Sends raw MAVLink data to the connected device.
    /// </summary>
    /// <param name="data">The raw MAVLink data to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    ValueTask SendRawAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);
}

//public abstract record MavLinkMessage(
//    byte SystemId,
//    byte ComponentId,
//    uint MessageId,
//    DateTimeOffset Timestamp);

//public sealed record HeartbeatMessage(
//    byte SystemId,
//    byte ComponentId,
//    uint CustomMode,
//    byte BaseMode,
//    byte SystemStatus)
//    : MavLinkMessage(
//        SystemId,
//        ComponentId,
//        0,
//        DateTimeOffset.UtcNow);

//public sealed record CommandAckMessage(
//    byte SystemId,
//    byte ComponentId,
//    ushort Command,
//    byte Result)
//    : MavLinkMessage(
//        SystemId,
//        ComponentId,
//        77,
//        DateTimeOffset.UtcNow);
