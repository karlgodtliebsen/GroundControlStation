using DroneGcs.Core.Models;
using DroneGcs.Core.Services;
using DroneGcs.Test.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace DroneGcs.Test.DomainEventsTests;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class DomainTests
{
    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public DomainTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Return_All_Simulated_Vehicles()
    {
        var services = TestConfigurator
            .AddTestConfiguration()
            .BuildServiceProvider();

        services.UseTestConfiguration();

        var registry = services.GetRequiredService<IVehicleRegistry>();
        var vehicleService = services.GetRequiredService<IVehicleService>();

        new SimulatedVehicleState
        {
            VehicleId = new VehicleId(1, 1),
            Latitude = 56.1629,
            Longitude = 10.2039,
            Altitude = 12.5,
            BatteryRemaining = 87,
            BatteryVoltage = 11.4f
        }.ApplyTo(registry);

        var vehicles = vehicleService.GetVehicles();

        var vehicle = Assert.Single(vehicles);

        Assert.Equal(new VehicleId(1, 1), vehicle.VehicleId);
        Assert.Equal(56.1629, vehicle.Latitude);
        Assert.Equal(10.2039, vehicle.Longitude);
        Assert.Equal(12.5, vehicle.Altitude);
        Assert.Equal(87, vehicle.BatteryRemaining);
        Assert.Equal(11.4f, vehicle.BatteryVoltage);
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Return_Specific_Simulated_Vehicle()
    {
        var services = TestConfigurator
            .AddTestConfiguration()
            .BuildServiceProvider();

        services.UseTestConfiguration();

        var registry = services.GetRequiredService<IVehicleRegistry>();
        var vehicleService = services.GetRequiredService<IVehicleService>();

        new SimulatedVehicleState
        {
            VehicleId = new VehicleId(1, 1),
            Roll = 0.1,
            Pitch = -0.2,
            Yaw = 1.5
        }.ApplyTo(registry);

        var vehicle = vehicleService.GetVehicle(new VehicleId(1, 1));

        Assert.Equal(new VehicleId(1, 1), vehicle.VehicleId);
        Assert.Equal(0.1, vehicle.Roll);
        Assert.Equal(-0.2, vehicle.Pitch);
        Assert.Equal(1.5, vehicle.Yaw);
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Should_Throw_When_Getting_Unknown_Vehicle()
    {
        var services = TestConfigurator
            .AddTestConfiguration()
            .BuildServiceProvider();

        services.UseTestConfiguration();

        var vehicleService = services.GetRequiredService<IVehicleService>();

        Assert.Throws<InvalidOperationException>(() => vehicleService.GetVehicle(new VehicleId(99, 1)));
    }
}
