using Domain.Library;

using DroneGcs.Core.Commands;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Logging;

namespace GroundControlStationApp.Configuration;

/// <summary>
/// Initializes the SITL (Software In The Loop) environment.
/// </summary>
/// <param name="vehicleService">The vehicle service.</param>
/// <param name="logger">The logger.</param>
public sealed class InitializeSitl(IVehicleService vehicleService, ILogger<InitializeSitl> logger)
{
    /// <summary>
    /// Initializes the SITL environment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="DomainException"></exception>
    public async Task Initialize(CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(1, 1);
        await EventuallyAsync(
            () =>
            {
                var vehicles = vehicleService.GetVehicles();

                logger.LogTrace("Vehicle count: {VehicleCount}", vehicles.Count);
                if (!vehicles.Any())
                {
                }
                //Assert.NotEmpty(vehicles);

                var vehicle = vehicles.First();

                logger.LogTrace("Vehicle: {VehicleId}, State: {ConnectionState}, Mode: {Mode}", vehicle.VehicleId, vehicle.ConnectionState, vehicle.Mode);
                vehicleId = vehicle.VehicleId;
                //Assert.Equal(VehicleConnectionState.Online, vehicle.ConnectionState);
                if (vehicle.ConnectionState != VehicleConnectionState.Online)
                {
                    throw new DomainException("vehicle.ConnectionState != VehicleConnectionState.Online");
                }
                // Handle unexpected mode
            },
            TimeSpan.FromSeconds(10),
            cancellationToken);

        var current = vehicleService.GetVehicleState(vehicleId);
        if (current is not null)
        {
            if (current.IsArmed)
            {
                var disarmResponse = await vehicleService.DisarmAsync(vehicleId, cancellationToken);

                //Assert.Equal(VehicleCommandResult.Accepted, disarmResponse.Result);
                if (disarmResponse.Result != VehicleCommandResult.Accepted)
                {
                    throw new DomainException("disarmResponse.Result != VehicleCommandResult.Accepted");
                }

                // Handle unexpected mode
                await EventuallyAsync(
                    () =>
                    {
                        var vehicle = vehicleService.GetVehicleState(vehicleId);
                        if (vehicle is not null)
                        {
                            //Assert.False(vehicle.IsArmed);
                            if (vehicle.IsArmed)
                            {
                                throw new DomainException("vehicle.IsArmed");
                            }
                        }
                        else
                        {
                            throw new DomainException("vehicle is null");
                        }
                    },
                    TimeSpan.FromSeconds(10),
                    cancellationToken);
            }
        }

        var modeResponse = await vehicleService.SetModeAsync(vehicleId, VehicleMode.Stabilize, cancellationToken);

        //Assert.Equal(VehicleCommandResult.Accepted, modeResponse.Result);
        if (modeResponse.Result != VehicleCommandResult.Accepted)
        {
            // Handle unexpected mode
            throw new DomainException("modeResponse.Result != VehicleCommandResult.Accepted");
        }

        await EventuallyAsync(
            () =>
            {
                var vehicle = vehicleService.GetVehicleState(vehicleId);
                if (vehicle is not null)
                {
                    //Assert.Equal(VehicleMode.Stabilize, vehicle.Mode);
                    //Assert.Equal(VehicleConnectionState.Online, vehicle.ConnectionState);
                    if (vehicle.Mode != VehicleMode.Stabilize)
                    {
                        // Handle unexpected mode
                        throw new DomainException("vehicle.Mode != VehicleMode.Stabilize");
                    }

                    if (vehicle.ConnectionState != VehicleConnectionState.Online)
                    {
                        // Handle unexpected mode
                        throw new DomainException("vehicle.ConnectionState != VehicleConnectionState.Online");
                    }
                }
            },
            TimeSpan.FromSeconds(10),
            cancellationToken);
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
