using Domain.Library.EventHub;

using DroneGcs.Core.Commands;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class DomainVehicleServiceTests
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainVehicleServiceTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public DomainVehicleServiceTests(ITestOutputHelper output)
    {
        this.output = output;
        var logger = NSubstitute.Substitute.For<ILogger<EventHub>>();

        var services = TestConfigurator.AddTestConfiguration();
        services.AddSingleton<ILogger<EventHub>>(logger);
        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();
    }

    /// <summary>
    /// Tests that a vehicle can be armed through IVehicleService using the full MAVLink simulator pipeline.
    /// </summary>
    [Fact]
    public async Task Should_Arm_Vehicle_Through_VehicleService_When_Command_Is_Acked()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider().UseTestConfiguration();

        var endpoint = services.GetRequiredService<IOptions<TransportEndpoint>>().Value;

        output.WriteLine($"UDP local:  {endpoint.LocalHost}:{endpoint.LocalPort}");
        output.WriteLine($"UDP remote: {endpoint.RemoteHost}:{endpoint.RemotePort}");

        var vehicleId = new VehicleId(1, 1);

        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await using var vehicleService = services.GetRequiredService<IVehicleService>();

        var messagePump = services.GetRequiredService<IVehicleMessagePump>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var pumpTask = Task.Run(
            () => messagePump.StartAsync(TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                services.GetRequiredService<IMavLinkFrameParser>(),
                services.GetRequiredService<IMavLinkCrcExtraProvider>(),
                endpoint.LocalHost,
                endpoint.LocalPort,
                endpoint.RemotePort,
                TimeSpan.FromMilliseconds(100)
            );

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);

                Assert.Equal(vehicleId, state.VehicleId);
                Assert.False(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var response = await vehicleService.ArmAsync(vehicleId, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Accepted, response.Result);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.True(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Disarm_Vehicle_Through_VehicleService_When_Command_Is_Acked()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider().UseTestConfiguration();

        var endpoint = services.GetRequiredService<IOptions<TransportEndpoint>>().Value;

        var vehicleId = new VehicleId(1, 1);

        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        var vehicleService = services.GetRequiredService<IVehicleService>();

        var messagePump = services.GetRequiredService<IVehicleMessagePump>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var pumpTask = Task.Run(() => messagePump.StartAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                services.GetRequiredService<IMavLinkFrameParser>(),
                services.GetRequiredService<IMavLinkCrcExtraProvider>(),
                endpoint.LocalHost,
                endpoint.LocalPort,
                endpoint.RemotePort,
                TimeSpan.FromMilliseconds(100)
            );

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.False(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var armResponse = await vehicleService.ArmAsync(vehicleId, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Accepted, armResponse.Result);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.True(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var disarmResponse = await vehicleService.DisarmAsync(vehicleId, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Accepted, disarmResponse.Result);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.False(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
    }


    /// <summary>
    /// Tests that the vehicle service correctly sets the vehicle mode to Guided when the command is acknowledged.
    /// </summary>
    [Fact]
    public async Task Should_Set_Guided_Mode_Through_VehicleService_When_Command_Is_Acked()
    {
        // same setup as arm/disarm integration test

        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider().UseTestConfiguration();

        var endpoint = services.GetRequiredService<IOptions<TransportEndpoint>>().Value;

        var vehicleId = new VehicleId(1, 1);

        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await using var vehicleService = services.GetRequiredService<IVehicleService>();

        var messagePump = services.GetRequiredService<IVehicleMessagePump>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var pumpTask = Task.Run(() => messagePump.StartAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                services.GetRequiredService<IMavLinkFrameParser>(),
                services.GetRequiredService<IMavLinkCrcExtraProvider>(),
                endpoint.LocalHost,
                endpoint.LocalPort,
                endpoint.RemotePort,
                TimeSpan.FromMilliseconds(100)
            );

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.Equal(VehicleMode.Stabilize, state.Mode);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var response = await vehicleService.SetModeAsync(vehicleId, VehicleMode.Guided, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Accepted, response.Result);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.Equal(VehicleMode.Guided, state.Mode);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the vehicle service correctly returns a timeout when the arm command is not acknowledged.
    /// </summary>
    [Fact]
    public async Task Should_Return_Timeout_When_Arm_Command_Is_Not_Acked()
    {
        var services = TestConfigurator.AddTestConfiguration().BuildServiceProvider().UseTestConfiguration();

        var endpoint = services.GetRequiredService<IOptions<TransportEndpoint>>().Value;

        var vehicleId = new VehicleId(1, 1);

        await using var connection = services.GetRequiredService<IMavLinkConnection>();

        await using var vehicleService = services.GetRequiredService<IVehicleService>();

        var messagePump = services.GetRequiredService<IVehicleMessagePump>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var pumpTask = Task.Run(() => messagePump.StartAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        await using var simulator =
            new FakeMavLinkVehicle2(
                services.GetRequiredService<IMavLinkFrameParser>(),
                services.GetRequiredService<IMavLinkCrcExtraProvider>(),
                endpoint.LocalHost,
                endpoint.LocalPort,
                endpoint.RemotePort,
                TimeSpan.FromMilliseconds(100),
                false);

        await simulator.StartAsync(TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicleId);
                Assert.Equal(vehicleId, state.VehicleId);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        var response = await vehicleService.ArmAsync(vehicleId, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Timeout, response.Result);
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
