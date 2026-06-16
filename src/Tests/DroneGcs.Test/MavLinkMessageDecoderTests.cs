using DroneGcs.Test.Configuration;

using DroneGs.MavLink.Client;
using DroneGs.MavLink.Decoding;
using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

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
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();


        await using var client = services.GetRequiredService<IMavLinkClient>();
        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        await using var simulator = new FakeMavLinkVehicle2(
            services.GetRequiredService<IMavLinkFrameParser>(),
            services.GetRequiredService<IMavLinkCrcExtraProvider>(), "127.0.0.1", 14550, TimeSpan.FromMilliseconds(100));

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
