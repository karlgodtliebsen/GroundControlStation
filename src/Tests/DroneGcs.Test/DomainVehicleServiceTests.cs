using Domain.Library.EventHub;

using DroneGcs.Core.Commands;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;
using DroneGcs.Test.Configuration;

using DroneGs.MavLink.Client;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        var services = TestConfigurator
            .AddTestConfiguration();
        services.AddSingleton<ILogger<EventHub>>(logger);
        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public async Task Should_Return_All_Simulated_VehiclesAsync()
    {
        var registry = serviceProvider.GetRequiredService<IVehicleRegistry>();
        var vehicleService = serviceProvider.GetRequiredService<IVehicleService>();

        var simulation = new SimulatedVehicleState
        {
            VehicleId = new VehicleId(1, 1),
            Latitude = 56.1629,
            Longitude = 10.2039,
            Altitude = 12.5,
            BatteryRemaining = 87,
            BatteryVoltage = 11.4f
        }.ApplyTo(registry);

        await using var client = serviceProvider.GetRequiredService<IMavLinkClient>();
        await using var connection = serviceProvider.GetRequiredService<IMavLinkConnection>();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var vehicles = vehicleService.GetVehicles();

        var vehicle = Assert.Single(vehicles);

        var response = await vehicleService.ArmAsync(vehicle.VehicleId, TestContext.Current.CancellationToken);

        Assert.Equal(VehicleCommandResult.Accepted, response.Result);

        await EventuallyAsync(
            () =>
            {
                var state = vehicleService.GetVehicle(vehicle.VehicleId);
                Assert.True(state.IsArmed);
            },
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);


        Assert.Equal(new VehicleId(1, 1), vehicle.VehicleId);
        Assert.Equal(56.1629, vehicle.Latitude);
        Assert.Equal(10.2039, vehicle.Longitude);
        Assert.Equal(12.5, vehicle.Altitude);
        Assert.Equal(87, vehicle.BatteryRemaining);
        Assert.Equal(11.4f, vehicle.BatteryVoltage);
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
