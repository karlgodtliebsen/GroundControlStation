using Domain.Library.EventHub.Abstractions;

using DroneGcs.Transport;

using DroneGs.MavLink.Client;

using Microsoft.Extensions.Logging;

namespace DroneGs.MavLink.Services;

public static class MavLinkEventTopics
{
    public const string ReceivedMessage = "mavlink.received-message";
    public const string ReceivedFrame = "mavlink.received-frame";
}

/// <summary>
/// Represents a connection to a MAVLink device, managing the reception and decoding of MAVLink frames and messages.
/// </summary>
public sealed class MavLinkConnection : IMavLinkConnection
{
    private readonly IMavLinkClient client;
    private readonly IMavLinkFrameParser frameParser;
    private readonly IMavLinkMessageDecoder messageDecoder;
    private readonly IEventHub eventHub;
    private readonly ILogger<MavLinkConnection> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MavLinkConnection"/> class with the specified dependencies. 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="frameParser"></param>
    /// <param name="messageDecoder"></param>
    /// <param name="eventHub"></param>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MavLinkConnection(IMavLinkClient client, IMavLinkFrameParser frameParser, IMavLinkMessageDecoder messageDecoder, IEventHub eventHub, ILogger<MavLinkConnection> logger)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.frameParser = frameParser ?? throw new ArgumentNullException(nameof(frameParser));
        this.messageDecoder = messageDecoder ?? throw new ArgumentNullException(nameof(messageDecoder));
        this.eventHub = eventHub ?? throw new ArgumentNullException(nameof(eventHub));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.client.DataReceived += OnDataReceivedAsync;
    }

    /// <summary>
    /// Starts the MAVLink connection, allowing it to receive and process incoming data.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await client.StartAsync(cancellationToken).ConfigureAwait(false);
        logger.LogDebug("MavLinkConnection - MAVLink connection started.");
    }


    /// <summary>
    /// Sends raw MAVLink data through the connection.
    /// </summary>
    /// <param name="data">The raw MAVLink data to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public async ValueTask SendRawAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("MavLinkConnection - Sending raw MAVLink data.");
        await client.SendAsync(data, cancellationToken).ConfigureAwait(false);
    }

    private async Task OnDataReceivedAsync(MavLinkDataReceived received, CancellationToken cancellationToken)
    {
        logger.LogDebug("MavLinkConnection - Data received at {ReceivedAt}", received.ReceivedAt);
        var parsedFrames = frameParser.Parse(received.Data.Span, received.ReceivedAt);

        foreach (var frame in parsedFrames)
        {
            // frames.Writer.TryWrite(frame);
            await eventHub.PublishAsync(MavLinkEventTopics.ReceivedFrame, frame, cancellationToken);
            if (messageDecoder.TryDecode(frame, out var message) && message is not null)
            {
                logger.LogDebug("MavLinkConnection - Writing Decoded Message { MessageType}", message.GetType().Name);
                await eventHub.PublishAsync(MavLinkEventTopics.ReceivedMessage, message, cancellationToken);
                return;
            }
        }
    }


    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        client.DataReceived -= OnDataReceivedAsync;

        // frames.Writer.TryComplete();
        //  messages.Writer.TryComplete();

        await client.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
        logger.LogDebug("MavLinkConnection - MAVLink connection disposed.");
    }
}
