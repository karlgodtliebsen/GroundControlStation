using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using DroneGs.MavLink;
using DroneGs.MavLink.Commands;
using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

namespace DroneGcs.Test;

/// <summary>
/// A simple MAVLink vehicle simulator that sends fake MAVLink messages over UDP.
/// </summary>
public sealed class FakeMavLinkVehicle2 : IAsyncDisposable
{
    private readonly UdpClient udpClient;
    private readonly IPEndPoint targetEndpoint;
    private readonly TimeSpan heartbeatInterval;
    private readonly IMavLinkFrameParser parser;

    private CancellationTokenSource? cancellationTokenSource;
    private Task? heartbeatTask;
    private Task? receiveTask;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetIp"></param>
    /// <param name="targetPort"></param>
    /// <param name="heartbeatInterval"></param>
    /// <param name="localPort"></param>
    public FakeMavLinkVehicle2(
        string targetIp,
        int targetPort,
        TimeSpan? heartbeatInterval = null,
        int localPort = 14551)
    {
        udpClient = new UdpClient(localPort);

        targetEndpoint = new IPEndPoint(
            IPAddress.Parse(targetIp),
            targetPort);

        this.heartbeatInterval =
            heartbeatInterval ?? TimeSpan.FromSeconds(1);

        parser = new MavLinkV2FrameParser(
            new CommonMavLinkCrcExtraProvider());
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        heartbeatTask = Task.Run(
            () => SendHeartbeatLoopAsync(cancellationTokenSource.Token),
            CancellationToken.None);

        receiveTask = Task.Run(
            () => ReceiveCommandLoopAsync(cancellationTokenSource.Token),
            CancellationToken.None);

        return Task.CompletedTask;
    }

    private async Task SendHeartbeatLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var heartbeat = MavLinkKnownFrames.CreateHeartbeatV2();

            await udpClient.SendAsync(
                    heartbeat,
                    targetEndpoint,
                    cancellationToken)
                .ConfigureAwait(false);

            await Task.Delay(
                    heartbeatInterval,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task ReceiveCommandLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await udpClient
                .ReceiveAsync(cancellationToken)
                .ConfigureAwait(false);

            var frames = parser.Parse(
                result.Buffer,
                DateTimeOffset.UtcNow);

            foreach (var frame in frames)
                if (frame.MessageId == MessageIds.CommandLong)
                    await HandleCommandLongAsync(
                            frame,
                            cancellationToken)
                        .ConfigureAwait(false);
        }
    }

    private async Task HandleCommandLongAsync(
        MavLinkFrame frame,
        CancellationToken cancellationToken)
    {
        if (frame.Payload.Length < 33) return;

        var payload = frame.Payload.Span;

        var command = BinaryPrimitives.ReadUInt16LittleEndian(payload[28..30]);

        if (command != MavLinkCommandIds.ComponentArmDisarm) return;

        var commandAck =
            CreateCommandAckV2(
                command,
                0); // MAV_RESULT_ACCEPTED

        await udpClient.SendAsync(
                commandAck,
                targetEndpoint,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static byte[] CreateCommandAckV2(
        ushort command,
        byte result)
    {
        Span<byte> payload = stackalloc byte[10];

        // MAVLink v2 COMMAND_ACK payload layout:
        //
        // uint16 command
        // uint8  result
        // uint8  progress
        // int32  result_param2
        // uint8  target_system
        // uint8  target_component

        BinaryPrimitives.WriteUInt16LittleEndian(
            payload[0..2],
            command);

        payload[2] = result;
        payload[3] = 0; // progress

        BinaryPrimitives.WriteInt32LittleEndian(
            payload[4..8],
            0); // result_param2

        payload[8] = 255; // target system, GCS
        payload[9] = 190; // target component, GCS

        return BuildV2Packet(
            1,
            1,
            MessageIds.CommandAck,
            payload);
    }

    private static byte[] BuildV2Packet(
        byte systemId,
        byte componentId,
        uint messageId,
        ReadOnlySpan<byte> payload)
    {
        var packet = new byte[10 + payload.Length + 2];

        packet[0] = 0xFD;
        packet[1] = (byte)payload.Length;
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = systemId;
        packet[6] = componentId;
        packet[7] = (byte)(messageId & 0xFF);
        packet[8] = (byte)((messageId >> 8) & 0xFF);
        packet[9] = (byte)((messageId >> 16) & 0xFF);

        payload.CopyTo(packet.AsSpan(10));

        var crcProvider = new CommonMavLinkCrcExtraProvider();

        if (!crcProvider.TryGetCrcExtra(messageId, out var crcExtra))
            throw new InvalidOperationException(
                $"No CRC extra registered for MAVLink message id {messageId}.");

        var crc = MavLinkCrc.Calculate(
            packet.AsSpan(1, 9 + payload.Length),
            crcExtra);

        var crcOffset = 10 + payload.Length;

        packet[crcOffset] = (byte)(crc & 0xFF);
        packet[crcOffset + 1] = (byte)((crc >> 8) & 0xFF);

        return packet;
    }

    public async ValueTask DisposeAsync()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync()
                .ConfigureAwait(false);

            await AwaitIgnoringCancellationAsync(heartbeatTask)
                .ConfigureAwait(false);

            await AwaitIgnoringCancellationAsync(receiveTask)
                .ConfigureAwait(false);

            cancellationTokenSource.Dispose();
        }

        udpClient.Dispose();
    }

    private static async Task AwaitIgnoringCancellationAsync(Task? task)
    {
        if (task is null) return;

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
