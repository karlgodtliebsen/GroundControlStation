using Domain.Library.Factory.Domain.Abstractions;
using DroneGcs.Core;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;
using DroneGs.MavLink;
using DroneGs.MavLink.Decoding;
using DroneGs.MavLink.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the <see cref="MavLinkMessageDecoder"/> class.
/// </summary>
public sealed class MavLinkMessageDecoderTests
{
    /// <summary>
    /// Tests that the <see cref="MavLinkMessageDecoder"/> can decode a heartbeat message from a fake vehicle.
    /// </summary>
    [Fact]
    public async Task Should_Decode_Heartbeat_Message_From_Fake_Vehicle()
    {
        //var parser = new MavLinkV2FrameParser(new CommonMavLinkCrcExtraProvider());
        //var decoder = new MavLinkMessageDecoder([new HeartbeatMessageDecoder()]);
        //var transport = new UdpMavLinkTransport(14550, "127.0.0.1", 14551);

        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        var parser = services.GetRequiredService<IMavLinkFrameParser>();
        var decoder = domainFactory.Create<IMavLinkMessageDecoder, IMavLinkMessageDecoder[]>([new HeartbeatMessageDecoder()]);
        var transport = domainFactory.Create<IMavLinkTransport, int, string, int>(14550, "127.0.0.1", 14551);

        await using var client = domainFactory.Create<IMavLinkClient, IMavLinkTransport>(transport);
        await using var connection = domainFactory.Create<IMavLinkConnection, IMavLinkClient, IMavLinkFrameParser, IMavLinkMessageDecoder>(client, parser, decoder);

        await connection.StartAsync(TestContext.Current.CancellationToken);

        await using var simulator = new FakeMavLinkVehicle2("127.0.0.1", 14550, TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        var message =
            await connection
                .ReadMessagesAsync(TestContext.Current.CancellationToken)
                .OfType<HeartbeatMessage>()
                .FirstAsync(TestContext.Current.CancellationToken)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, message.SystemId);
        Assert.Equal(1, message.ComponentId);
        Assert.Equal(0u, message.CustomMode);
        Assert.Equal(2, message.VehicleType);
        Assert.Equal(3, message.Autopilot);
        Assert.Equal(0, message.BaseMode);
        Assert.Equal(4, message.SystemStatus);
        Assert.Equal(3, message.MavLinkVersion);
    }
}
