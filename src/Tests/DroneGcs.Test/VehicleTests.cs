using Domain.Library.Factory.Domain.Abstractions;

using DroneGcs.Core;
using DroneGcs.Core.MavLink;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink;
using DroneGs.MavLink.Decoder;
using DroneGs.MavLink.Messages;

using Microsoft.Extensions.DependencyInjection;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the MAVLink client and transport implementations.
/// </summary>
public class VehicleTests
{
    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public VehicleTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// Tests that a vehicle is registered when a heartbeat message is received.
    /// </summary>
    [Fact]
    public void Should_Register_Vehicle_When_Heartbeat_Is_Received()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = domainFactory.Create<IHeartbeatVehicleHandler, IVehicleRegistry>(registry);

        var heartbeat = new HeartbeatMessage(
            1,
            1,
            0,
            2,
            3,
            0,
            4,
            3,
            DateTimeOffset.UtcNow);

        var vehicle = handler.Handle(heartbeat);

        Assert.Equal(new VehicleId(1, 1), vehicle.Id);
        Assert.Single(registry.Vehicles);
        Assert.Equal(VehicleConnectionState.Online, vehicle.State.ConnectionState);
        Assert.Equal(2, vehicle.State.VehicleType);
        Assert.Equal(3, vehicle.State.Autopilot);
    }

    /// <summary>
    /// Tests that a vehicle is registered when a heartbeat message is received from the MAVLink connection.
    /// </summary>
    [Fact]
    public async Task Should_Register_Vehicle_From_Received_Heartbeat_MessageAsync()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        var parser = services.GetRequiredService<IMavLinkFrameParser>();
        var decoder = domainFactory.Create<IMavLinkMessageDecoder, IMavLinkMessageDecoder[]>([new HeartbeatMessageDecoder()]);
        var transport = domainFactory.Create<IMavLinkTransport, int, string, int>(14550, "127.0.0.1", 14551);

        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = domainFactory.Create<IHeartbeatVehicleHandler, IVehicleRegistry>(registry);

        await using var client = domainFactory.Create<IMavLinkClient, IMavLinkTransport>(transport);
        await using var connection = domainFactory.Create<IMavLinkConnection, IMavLinkClient, IMavLinkFrameParser, IMavLinkMessageDecoder>(client, parser, decoder);

        await connection.StartAsync(TestContext.Current.CancellationToken);

        output.WriteLine($"Client IsRunning: {client.IsRunning}");
        output.WriteLine($"Transport IsConnected: {client.IsConnected}");
        await using var simulator =
            new FakeMavLinkVehicle2(
                "127.0.0.1",
                14550,
                TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        var message = await connection
            .ReadMessagesAsync(TestContext.Current.CancellationToken)
            .OfType<HeartbeatMessage>()
            .FirstAsync(TestContext.Current.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var vehicle = handler.Handle(message);

        Assert.Equal(new VehicleId(1, 1), vehicle.Id);
        Assert.Single(registry.Vehicles);
        Assert.Equal(VehicleConnectionState.Online, vehicle.State.ConnectionState);
    }


    /// <summary>
    /// Tests that an existing vehicle is updated when a repeated heartbeat message is received.
    /// </summary>
    [Fact]
    public void Should_Update_Existing_Vehicle_When_Heartbeat_Is_Repeated()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();

        var domainFactory = services.GetRequiredService<IDomainFactory>();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = domainFactory.Create<IHeartbeatVehicleHandler, IVehicleRegistry>(registry);

        var first = new HeartbeatMessage(
            1, 1, 0, 2, 3, 0, 4, 3,
            DateTimeOffset.UtcNow);

        var second = new HeartbeatMessage(
            1, 1, 42, 2, 3, 81, 4, 3,
            DateTimeOffset.UtcNow.AddSeconds(1));

        var vehicle1 = handler.Handle(first);
        var vehicle2 = handler.Handle(second);

        Assert.Same(vehicle1, vehicle2);
        Assert.Single(registry.Vehicles);
        Assert.Equal(42u, vehicle2.State.CustomMode);
        Assert.Equal(81, vehicle2.State.BaseMode);
    }

    [Fact]
    public async Task Should_Register_Vehicle_When_Message_Pump_Receives_Heartbeat()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();

        var domainFactory = services.GetRequiredService<IDomainFactory>();

        var parser = services.GetRequiredService<IMavLinkFrameParser>();
        var decoder = domainFactory.Create<IMavLinkMessageDecoder, IMavLinkMessageDecoder[]>(
            [new HeartbeatMessageDecoder()]);

        var transport = domainFactory.Create<IMavLinkTransport, int, string, int>(
            14550, "127.0.0.1", 14551);

        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = services.GetRequiredService<IHeartbeatVehicleHandler>();

        await using var client =
            domainFactory.Create<IMavLinkClient, IMavLinkTransport>(transport);

        await using var connection =
            domainFactory.Create<IMavLinkConnection, IMavLinkClient, IMavLinkFrameParser, IMavLinkMessageDecoder>(
                client,
                parser,
                decoder);

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var pump = domainFactory.Create<IVehicleMessagePump, IMavLinkConnection, IHeartbeatVehicleHandler>(
            connection,
            handler);

        var pumpTask = pump.StartAsync(TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                "127.0.0.1",
                14550,
                TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () => Assert.Single(registry.Vehicles),
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        Assert.Contains(
            registry.Vehicles,
            vehicle => vehicle.Id == new VehicleId(1, 1));
    }


    /// <summary>
    /// Tests that a vehicle is marked as stale when its heartbeat is old.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_As_Stale_When_Heartbeat_Is_Old()
    {
        var registry = new VehicleRegistry();

        var receivedAt = DateTimeOffset.UtcNow;

        var vehicle = registry.RegisterOrUpdateHeartbeat(
            new VehicleId(1, 1),
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt);

        registry.UpdateConnectionStates(receivedAt.AddSeconds(3), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));

        Assert.Equal(
            VehicleConnectionState.Stale,
            vehicle.State.ConnectionState);
    }

    /// <summary>
    /// Tests that a vehicle is marked as offline when its heartbeat is very old.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_As_Offline_When_Heartbeat_Is_Very_Old()
    {
        var registry = new VehicleRegistry();

        var receivedAt = DateTimeOffset.UtcNow;

        var vehicle = registry.RegisterOrUpdateHeartbeat(
            new VehicleId(1, 1),
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt);

        registry.UpdateConnectionStates(
            receivedAt.AddSeconds(6),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5));

        Assert.Equal(
            VehicleConnectionState.Offline,
            vehicle.State.ConnectionState);
    }

    /// <summary>
    /// Tests that a vehicle is marked as online when a new heartbeat arrives after it was marked offline.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_Online_When_New_Heartbeat_Arrives_After_Offline()
    {
        var registry = new VehicleRegistry();

        var receivedAt = DateTimeOffset.UtcNow;
        var vehicleId = new VehicleId(1, 1);

        var vehicle = registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt);

        registry.UpdateConnectionStates(
            receivedAt.AddSeconds(6),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5));

        Assert.Equal(
            VehicleConnectionState.Offline,
            vehicle.State.ConnectionState);

        registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt.AddSeconds(7));

        Assert.Equal(
            VehicleConnectionState.Online,
            vehicle.State.ConnectionState);
    }

    private static async Task EventuallyAsync(Action assertion, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay(50, cancellationToken);
            }
        }

        throw lastException ?? new TimeoutException();
    }
}
