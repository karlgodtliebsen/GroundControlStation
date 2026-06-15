using System.Threading.Channels;

using DroneGcs.Transport;

using DroneGs.MavLink.Client;
using DroneGs.MavLink.Messages;

namespace DroneGs.MavLink.Services;

/// <summary>
/// Represents a connection to a MAVLink device, managing the reception and decoding of MAVLink frames and messages.
/// </summary>
public sealed class MavLinkConnection : IMavLinkConnection
{
    private readonly MavLinkClient client;
    private readonly IMavLinkFrameParser frameParser;
    private readonly IMavLinkMessageDecoder messageDecoder;

    private readonly Channel<MavLinkFrame> frames =
        Channel.CreateUnbounded<MavLinkFrame>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = true
            });

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="frameParser"></param>
    /// <param name="messageDecoder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MavLinkConnection(MavLinkClient client, IMavLinkFrameParser frameParser, IMavLinkMessageDecoder messageDecoder)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.frameParser = frameParser ?? throw new ArgumentNullException(nameof(frameParser));
        this.messageDecoder = messageDecoder ?? throw new ArgumentNullException(nameof(messageDecoder));

        this.client.DataReceived += OnDataReceivedAsync;
    }

    /// <summary>
    /// Starts the MAVLink connection, allowing it to receive and process incoming data.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await client.StartAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Reads MAVLink frames from the connection as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<MavLinkFrame> ReadFramesAsync(CancellationToken cancellationToken = default)
    {
        return frames.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    /// Reads MAVLink messages from the connection as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<MavLinkMessage> ReadMessagesAsync(CancellationToken cancellationToken = default)
    {
        return messages.Reader.ReadAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SendAsync(MavLinkMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sends raw MAVLink data through the connection.
    /// </summary>
    /// <param name="data">The raw MAVLink data to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public async ValueTask SendRawAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        await client.SendAsync(data, cancellationToken)
            .ConfigureAwait(false);
    }

    private ValueTask OnDataReceivedAsync(MavLinkDataReceived received, CancellationToken cancellationToken)
    {
        var parsedFrames = frameParser.Parse(
            received.Data.Span,
            received.ReceivedAt);

        foreach (var frame in parsedFrames)
        {
            frames.Writer.TryWrite(frame);

            if (messageDecoder.TryDecode(frame, out var message) && message is not null)
            {
                messages.Writer.TryWrite(message);
            }
        }

        return ValueTask.CompletedTask;
    }


    private readonly Channel<MavLinkMessage> messages = Channel.CreateUnbounded<MavLinkMessage>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = true
    });

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        client.DataReceived -= OnDataReceivedAsync;

        frames.Writer.TryComplete();
        messages.Writer.TryComplete();

        await client.DisposeAsync()
            .ConfigureAwait(false);
    }
}
