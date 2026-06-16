using System.Buffers.Binary;

using Domain.Library.EventHub.Abstractions;
using Domain.Library.Factory.Domain.Abstractions;

using DroneGcs.Core.Models;
using DroneGcs.Core.Services;
using DroneGcs.Core.VehicleHandler;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink;
using DroneGs.MavLink.Client;
using DroneGs.MavLink.Commands;
using DroneGs.MavLink.Decoding;
using DroneGs.MavLink.Encoding;
using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = services.GetRequiredService<IHeartbeatVehicleHandler>();
        await using var client = services.GetRequiredService<IMavLinkClient>();
        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        output.WriteLine($"Client IsRunning: {client.IsRunning}");
        output.WriteLine($"Transport IsConnected: {client.IsConnected}");
        await using var simulator = new FakeMavLinkVehicle2(
            services.GetRequiredService<IMavLinkFrameParser>(),
            services.GetRequiredService<IMavLinkCrcExtraProvider>(), "127.0.0.1", 14550, 14551, TimeSpan.FromMilliseconds(100));

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

    /// <summary>
    /// Tests that a vehicle is registered when the message pump receives a heartbeat message.
    /// </summary>
    [Fact]
    public async Task Should_Register_Vehicle_When_Message_Pump_Receives_Heartbeat()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();

        var registry = services.GetRequiredService<IVehicleRegistry>();

        await using var client = services.GetRequiredService<IMavLinkClient>();
        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);


        //var pump = domainFactory.Create<IVehicleMessagePump, IMavLinkConnection,
        //    IHeartbeatVehicleHandler, IPositionVehicleHandler, IAttitudeVehicleHandler, IBatteryVehicleHandler>(
        //    connection,
        //    services.GetRequiredService<IHeartbeatVehicleHandler>(),
        //    services.GetRequiredService<IPositionVehicleHandler>(),
        //    services.GetRequiredService<IAttitudeVehicleHandler>(),
        //    services.GetRequiredService<IBatteryVehicleHandler>());
        var pump = services.GetRequiredService<IVehicleMessagePump>();

        var pumpTask = pump.StartAsync(TestContext.Current.CancellationToken);
        await using var simulator = new FakeMavLinkVehicle2(
            services.GetRequiredService<IMavLinkFrameParser>(),
            services.GetRequiredService<IMavLinkCrcExtraProvider>(), "127.0.0.1", 14550, 14551, TimeSpan.FromMilliseconds(100));


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
    /// Tests that a vehicle's position is updated when a GlobalPositionInt message is received.
    /// </summary>
    [Fact]
    public void Should_Update_Vehicle_Position_From_GlobalPositionInt_Message()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var domainFactory = services.GetRequiredService<IDomainFactory>();

        var vehicleId = new VehicleId(1, 1);

        registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            DateTimeOffset.UtcNow);

        var handler = domainFactory.Create<IPositionVehicleHandler, IVehicleRegistry>(registry);

        handler.Handle(
            new GlobalPositionIntMessage(
                1,
                1,
                56.1629,
                10.2039,
                12.5,
                DateTimeOffset.UtcNow));

        var vehicle = registry.GetRequired(vehicleId);

        Assert.Equal(56.1629, vehicle.State.Latitude);
        Assert.Equal(10.2039, vehicle.State.Longitude);
        Assert.Equal(12.5, vehicle.State.Altitude);
    }

    /// <summary>
    /// Tests that a vehicle's battery is updated when a SysStatusMessage is received.
    /// </summary>
    [Fact]
    public void Should_Update_Battery_From_SysStatusMessage_Message()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var domainFactory = services.GetRequiredService<IDomainFactory>();

        var vehicleId = new VehicleId(1, 1);

        registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            DateTimeOffset.UtcNow);

        var handler = domainFactory.Create<IBatteryVehicleHandler, IVehicleRegistry>(registry);

        handler.Handle(
            new SysStatusMessage(
                1,
                1,
                56,
                (float)10.0,
                DateTimeOffset.UtcNow));

        var vehicle = registry.GetRequired(vehicleId);

        Assert.Equal(56, vehicle.State.BatteryRemaining);
        Assert.Equal((float)10.0, vehicle.State.BatteryVoltage);
    }

    /// <summary>
    /// Tests that a vehicle's attitude is updated when an AttitudeMessage is received.
    /// </summary>
    [Fact]
    public void Should_Update_Attitude_From_AttitudeMessage()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var domainFactory = services.GetRequiredService<IDomainFactory>();

        var vehicleId = new VehicleId(1, 1);

        registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            DateTimeOffset.UtcNow);

        var handler = domainFactory.Create<IAttitudeVehicleHandler, IVehicleRegistry>(registry);

        handler.Handle(
            new AttitudeMessage(
                1,
                1,
                56.1629,
                10.2039,
                12.5,
                DateTimeOffset.UtcNow));

        var vehicle = registry.GetRequired(vehicleId);

        Assert.Equal(56.1629, vehicle.State.Roll);
        Assert.Equal(10.2039, vehicle.State.Pitch);
        Assert.Equal(12.5, vehicle.State.Yaw);
    }


    /// <summary>
    /// Tests that a vehicle is marked as stale when its heartbeat is old.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_As_Stale_When_Heartbeat_Is_Old()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var eventHub = services.GetRequiredService<IEventHub>();

        var registry = services.GetRequiredService<IVehicleRegistry>();

        var receivedAt = DateTimeOffset.UtcNow;

        var vehicleRegistryResult = registry.RegisterOrUpdateHeartbeat(
            new VehicleId(1, 1),
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt);

        registry.UpdateConnectionStates(receivedAt.AddSeconds(3), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));

        Assert.Equal(VehicleConnectionState.Stale,
            vehicleRegistryResult.Vehicle.State.ConnectionState);
    }

    /// <summary>
    /// Tests that a vehicle is marked as offline when its heartbeat is very old.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_As_Offline_When_Heartbeat_Is_Very_Old()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();

        var receivedAt = DateTimeOffset.UtcNow;

        var vehicleRegistryResult = registry.RegisterOrUpdateHeartbeat(
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
            vehicleRegistryResult.Vehicle.State.ConnectionState);
    }

    /// <summary>
    /// Tests that a vehicle is marked as online when a new heartbeat arrives after it was marked offline.
    /// </summary>
    [Fact]
    public void Should_Mark_Vehicle_Online_When_New_Heartbeat_Arrives_After_Offline()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();


        var receivedAt = DateTimeOffset.UtcNow;
        var vehicleId = new VehicleId(1, 1);

        var vehicleRegistryResult = registry.RegisterOrUpdateHeartbeat(
            vehicleId,
            0,
            2,
            3,
            0,
            4,
            3,
            receivedAt);

        registry.UpdateConnectionStates(receivedAt.AddSeconds(6), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));

        Assert.Equal(
            VehicleConnectionState.Offline,
            vehicleRegistryResult.Vehicle.State.ConnectionState);

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
            vehicleRegistryResult.Vehicle.State.ConnectionState);
    }

    /// <summary>
    /// Tests that an arm command is correctly encoded into a MAVLink CommandLong frame. 
    /// </summary>
    [Fact]
    public void Should_Encode_Arm_CommandLong_Frame()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var encoder = services.GetRequiredService<IMavLinkCommandEncoder>();

        var packet = encoder.EncodeArmDisarm(
            1,
            1,
            true);

        Assert.Equal(0xFD, packet[0]);
        Assert.Equal(33, packet[1]);

        var messageId =
            packet[7]
            | ((uint)packet[8] << 8)
            | ((uint)packet[9] << 16);

        Assert.Equal(MessageIds.CommandLong, messageId);

        var payload = packet.AsSpan(10, 33);

        var param1 = BitConverter.ToSingle(payload[0..4]);
        var command = BinaryPrimitives.ReadUInt16LittleEndian(payload[28..30]);

        Assert.Equal(1.0f, param1);
        Assert.Equal(MavLinkCommandIds.ComponentArmDisarm, command);
        Assert.Equal(1, payload[30]); // target system
        Assert.Equal(1, payload[31]); // target component
    }

    /// <summary>
    /// Tests that a CommandAck message is correctly decoded from a MAVLink frame. 
    /// </summary>
    [Fact]
    public void Should_Decode_CommandAck_Message()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();

        var receivedAt = DateTimeOffset.UtcNow;
        var payload = new byte[]
        {
            0x90, 0x01, // 400 COMPONENT_ARM_DISARM
            0x00 // ACCEPTED
        };

        var frame = new MavLinkFrame(
            1,
            1,
            MessageIds.CommandAck,
            0,
            payload,
            new ReadOnlyMemory<byte>(),
            receivedAt);

        var decoder = new CommandAckMessageDecoder();

        var decoded = decoder.TryDecode(frame, out var message);

        Assert.True(decoded);

        var ack = Assert.IsType<CommandAckMessage>(message);

        Assert.Equal(1, ack.SystemId);
        Assert.Equal(1, ack.ComponentId);
        Assert.Equal(400, ack.Command);
        Assert.Equal(0, ack.Result);
    }

    /// <summary>
    /// Tests that sending an arm command results in receiving a command acknowledgment from the simulator. 
    /// </summary>
    [Fact]
    public async Task Should_Receive_CommandAck_When_Arm_Command_Is_Sent()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();

        var options = services.GetRequiredService<IOptions<TransportEndpoint>>();
        var endpoint = options.Value;

        output.WriteLine($"UDP local: {endpoint.LocalHost}:{endpoint.LocalPort}");
        output.WriteLine($"UDP remote: {endpoint.RemoteHost}:{endpoint.RemotePort}");

        await using var client = services.GetRequiredService<IMavLinkClient>();
        await using var connection = services.GetRequiredService<IMavLinkConnection>();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                services.GetRequiredService<IMavLinkFrameParser>(),
                services.GetRequiredService<IMavLinkCrcExtraProvider>(),
                endpoint.LocalHost,
                endpoint.LocalPort,
                endpoint.RemotePort, TimeSpan.FromMilliseconds(100));

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        var encoder = services.GetRequiredService<IMavLinkCommandEncoder>();

        var armCommand = encoder.EncodeArmDisarm(1, 1, true);

        await connection.SendRawAsync(armCommand, TestContext.Current.CancellationToken);

        var ack = await connection
            .ReadMessagesAsync(TestContext.Current.CancellationToken)
            .OfType<CommandAckMessage>()
            .FirstAsync(TestContext.Current.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, ack.SystemId);
        Assert.Equal(1, ack.ComponentId);
        Assert.Equal(MavLinkCommandIds.ComponentArmDisarm, ack.Command);
        Assert.Equal(0, ack.Result);
    }

    /// <summary>
    /// Tests that the armed state is correctly updated from the heartbeat base mode.
    /// </summary>
    [Fact]
    public void Should_Update_Armed_State_From_Heartbeat_BaseMode()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = services.GetRequiredService<IHeartbeatVehicleHandler>();
        var heartbeat = new HeartbeatMessage(
            1, 1,
            0,
            2,
            3,
            128,
            4,
            3,
            DateTimeOffset.UtcNow);

        var vehicle = handler.Handle(heartbeat);

        Assert.True(vehicle.State.IsArmed);
    }

    /// <summary>
    /// Tests that the vehicle mode is correctly updated from the heartbeat custom mode. 
    /// </summary>
    [Fact]
    public void Should_Update_Mode_From_Heartbeat_CustomMode()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var domainFactory = services.GetRequiredService<IDomainFactory>();
        var registry = services.GetRequiredService<IVehicleRegistry>();
        var handler = domainFactory.Create<IHeartbeatVehicleHandler, IVehicleRegistry>(registry);

        var heartbeat = new HeartbeatMessage(
            1, 1,
            4,
            2,
            3,
            0,
            4,
            3,
            DateTimeOffset.UtcNow);

        var vehicle = handler.Handle(heartbeat);

        Assert.Equal(VehicleMode.Guided, vehicle.State.Mode);
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Update_Position()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var vehicle = CreateVehicleSession();

        vehicle.ApplyPosition(
            56.1629,
            10.2039,
            12.5);

        Assert.Equal(56.1629, vehicle.State.Latitude);
        Assert.Equal(10.2039, vehicle.State.Longitude);
        Assert.Equal(12.5, vehicle.State.Altitude);
    }


    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Update_Attitude()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var vehicle = CreateVehicleSession();

        vehicle.ApplyAttitude(0.1, -0.2, 1.5);

        Assert.Equal(0.1, vehicle.State.Roll);
        Assert.Equal(-0.2, vehicle.State.Pitch);
        Assert.Equal(1.5, vehicle.State.Yaw);
    }


    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Update_Battery()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider();
        services.UseTestConfiguration();
        var vehicle = CreateVehicleSession();

        vehicle.ApplyBattery(87, 11.4f);

        Assert.Equal(87, vehicle.State.BatteryRemaining);
        Assert.Equal(11.4f, vehicle.State.BatteryVoltage);
    }


    private static VehicleSession CreateVehicleSession()
    {
        var state = new VehicleState(
            new VehicleId(1, 1),
            0,
            2,
            3,
            0,
            4,
            3,
            VehicleConnectionState.Online,
            DateTimeOffset.UtcNow,
            VehicleMode.Stabilize,
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        return new VehicleSession(state);
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
