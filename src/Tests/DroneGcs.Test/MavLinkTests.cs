using DroneGcs.Test.Configuration;

using DroneGs.MavLink;
using DroneGs.MavLink.Client;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the MAVLink client and transport implementations.
/// </summary>
public class MavLinkTests
{
    private readonly ITestOutputHelper output;
    //UDP transport
    //Receive loop
    //Events
    //Cancellation
    //Lifecycle

    public MavLinkTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// Tests that the MAVLink client can receive data from a fake vehicle.
    /// </summary>
    [Fact]
    public async Task Should_Receive_Data_From_Fake_Vehicle()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        //var domainFactory = services.GetRequiredService<IDomainFactory>();
        //var transport = services.GetRequiredService<IMavLinkTransport>();
        // await using var client = domainFactory.Create<IMavLinkClient, IMavLinkTransport>(transport);

        var received = new TaskCompletionSource<byte[]>();
        await using var client = services.GetRequiredService<IMavLinkClient>();

        client.DataReceived += (data, _) =>
        {
            received.TrySetResult(data.Data.ToArray());

            return ValueTask.CompletedTask;
        };

        await client.StartAsync(TestContext.Current.CancellationToken);

        await using FakeMavLinkVehicle simulator = new("127.0.0.1", 14550);

        await simulator.StartAsync();

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotEmpty(result);

        await client.StopAsync();
    }

    /// <summary>
    /// Tests that the MAVLink client can receive a valid MAVLink v2 heartbeat frame from a fake vehicle.
    /// </summary>
    [Fact]
    public async Task Should_Receive_Valid_MavLinkV2_Heartbeat_Frame()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        // var domainFactory = services.GetRequiredService<IDomainFactory>();
        //var transport = services.GetRequiredService<IMavLinkTransport>();

        //await using var client = domainFactory.Create<IMavLinkClient, IMavLinkTransport>(transport);

        var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var client = services.GetRequiredService<IMavLinkClient>();

        client.DataReceived += (data, _) =>
        {
            received.TrySetResult(data.Data.ToArray());
            return ValueTask.CompletedTask;
        };

        await client.StartAsync(TestContext.Current.CancellationToken);

        await using var simulator = new FakeMavLinkVehicle2(
            services.GetRequiredService<IMavLinkFrameParser>(),
            services.GetRequiredService<IMavLinkCrcExtraProvider>(), "127.0.0.1", 14550, TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        var frame = await received.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(0xFD, frame[0]); // MAVLink v2
        Assert.Equal(9, frame[1]); // HEARTBEAT payload length
        Assert.Equal(1, frame[5]); // SystemId
        Assert.Equal(1, frame[6]); // ComponentId
        Assert.Equal(0u, GetMessageId(frame));

        await client.StopAsync();
    }

    private static uint GetMessageId(byte[] frame)
    {
        return
            frame[7]
            | ((uint)frame[8] << 8)
            | ((uint)frame[9] << 16);
    }


    /// <summary>
    /// Tests that the MAVLink client can calculate a valid CRC for a known MAVLink v2 heartbeat frame.
    /// </summary>
    [Fact]
    public void Should_Have_Valid_Crc_For_Known_Heartbeat()
    {
        var frame = MavLinkKnownFrames.CreateHeartbeatV2();

        var payloadLength = frame[1];
        var messageId =
            frame[7]
            | ((uint)frame[8] << 8)
            | ((uint)frame[9] << 16);

        var provider = new CommonMavLinkCrcExtraProvider();

        Assert.True(provider.TryGetCrcExtra(messageId, out var crcExtra));

        var calculatedCrc = MavLinkCrc.Calculate(
            frame.AsSpan(1, 9 + payloadLength),
            crcExtra);

        var receivedCrcOffset = 10 + payloadLength;

        var receivedCrc =
            (ushort)(frame[receivedCrcOffset]
                     | (frame[receivedCrcOffset + 1] << 8));

        Assert.Equal(calculatedCrc, receivedCrc);
    }

    /// <summary>
    /// Tests that the MAVLink client can parse a valid MAVLink v2 heartbeat frame from a fake vehicle.
    /// </summary>
    [Fact]
    public async Task Should_Parse_Heartbeat_Frame_From_Fake_Vehicle()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        await using var client = services.GetRequiredService<IMavLinkClient>();
        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        // Add debug output
        output.WriteLine($"Client IsRunning: {client.IsRunning}");
        output.WriteLine($"Transport IsConnected: {client.IsConnected}");

        await using var simulator = new FakeMavLinkVehicle2(
            services.GetRequiredService<IMavLinkFrameParser>(),
            services.GetRequiredService<IMavLinkCrcExtraProvider>(), "127.0.0.1", 14550, TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await Task.Delay(500, TestContext.Current.CancellationToken);

        output.WriteLine($"Client still running: {client.IsRunning}");

        var frame = await connection.ReadFramesAsync(TestContext.Current.CancellationToken)
            .FirstAsync(TestContext.Current.CancellationToken).AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, frame.SystemId);
        Assert.Equal(1, frame.ComponentId);
        Assert.Equal(0u, frame.MessageId);
        Assert.Equal(0, frame.Sequence);
        Assert.Equal(0xFD, frame.RawBytes.Span[0]);
    }

    /// <summary>
    /// Tests that the MAVLink parser rejects a frame with an invalid CRC.
    /// </summary>
    [Fact]
    public void Should_Reject_Frame_With_Invalid_Crc()
    {
        var parser = new MavLinkV2FrameParser(new CommonMavLinkCrcExtraProvider());

        var frame = MavLinkKnownFrames.CreateHeartbeatV2();
        frame[^1] ^= 0xFF;

        var frames = parser.Parse(frame, DateTimeOffset.UtcNow);

        Assert.Empty(frames);
    }
}


//DroneGcs.Core.Tests
//    ↓
//FakeMavLinkVehicle
//    ↓ UDP loopback
//DroneGcs.MavLink
