using Domain.Library;

using DroneGcs.ConsoleHost.Dashboard;
using DroneGcs.Core.Commands;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.WorkerServices;

/// <summary>
/// A worker service that can run a single task.
/// </summary>
public class CommandWorkerService(IVehicleService vehicleService, CommandOutputBuffer commandOutputBuffer, IHostApplicationLifetime lifetime, ILogger<CommandWorkerService> logger) : ICommandWorkerService
{
    private async Task ResetSitlVehicleAsync(CancellationToken cancellationToken)
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

    /// <inheritdoc />
    public async Task Run(CancellationToken stoppingToken)
    {
        logger.LogInformation("Console command service started.");
        WriteHelp();
        await ResetSitlVehicleAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            commandOutputBuffer.Add("> ");

            var line = await Console.In.ReadLineAsync(stoppingToken)
                .ConfigureAwait(false);

            //var line = AnsiConsole.Ask<string>("[green]>[/]");
            //var line = await Task.Run(() => AnsiConsole.Prompt(new TextPrompt<string>("[green]>[/]").AllowEmpty()), stoppingToken);

            //CommandOutputBuffer.Add("Arm result: " + response.Result);


            if (line is null)
            {
                continue;
            }

            line = line.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) || line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                lifetime.StopApplication();
                return;
            }

            try
            {
                await ExecuteCommandAsync(line, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Command failed: {Command}", line);
            }
        }
    }

    private async Task ExecuteCommandAsync(string line, CancellationToken cancellationToken)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
            case "help":
                WriteHelp();
                break;

            case "vehicles":
                WriteVehicles();
                break;

            case "state":
                WriteState(parts);
                break;

            case "arm":
                await ArmAsync(parts, cancellationToken);
                break;

            case "disarm":
                await DisarmAsync(parts, cancellationToken);
                break;

            case "mode":
                await SetModeAsync(parts, cancellationToken);
                break;

            default:
                commandOutputBuffer.Add($"Unknown command: {command}");
                commandOutputBuffer.Add("Type 'help' for commands.");
                break;
        }
    }

    private void WriteVehicles()
    {
        var vehicles = vehicleService.GetVehicles();

        if (vehicles.Count == 0)
        {
            commandOutputBuffer.Add("No vehicles registered.");
            return;
        }

        foreach (var vehicle in vehicles)
        {
            commandOutputBuffer.Add(
                $"{Format(vehicle.VehicleId)}  " +
                $"State={vehicle.ConnectionState}, " +
                $"Mode={vehicle.Mode}, " +
                $"Armed={vehicle.IsArmed}, " +
                $"Battery={vehicle.BatteryRemaining?.ToString() ?? "-"}%");
        }
    }

    private void WriteState(string[] parts)
    {
        if (!TryGetVehicleId(parts, out var vehicleId))
        {
            commandOutputBuffer.Add("Usage: state <systemId>:<componentId>");
            return;
        }

        var vehicle = vehicleService.GetVehicle(vehicleId);

        if (vehicle is null)
        {
            commandOutputBuffer.Add("Vehicle not found.");
            return;
        }

        var vehicleState = vehicleService.GetVehicleState(vehicleId);
        if (vehicleState is null)
        {
            return;
        }

        commandOutputBuffer.Add($"Vehicle: {Format(vehicleState.VehicleId)}");
        commandOutputBuffer.Add($"Connection: {vehicleState.ConnectionState}");
        commandOutputBuffer.Add($"Mode: {vehicleState.Mode}");
        commandOutputBuffer.Add($"Armed: {vehicleState.IsArmed}");
        commandOutputBuffer.Add($"Lat/Lon/Alt: {vehicleState.Latitude}, {vehicleState.Longitude}, {vehicleState.Altitude}");
        commandOutputBuffer.Add($"Roll/Pitch/Yaw: {vehicleState.Roll}, {vehicleState.Pitch}, {vehicleState.Yaw}");
        commandOutputBuffer.Add($"Battery: {vehicleState.BatteryVoltage}V / {vehicleState.BatteryRemaining}%");


        foreach (var message in vehicle.Notifications.TakeLast(10))
        {
            commandOutputBuffer.Add($"[{message.ReceivedAt:HH:mm:ss}] : {message.Text}");
        }
    }

    private async Task ArmAsync(string[] parts, CancellationToken cancellationToken)
    {
        if (!TryGetVehicleId(parts, out var vehicleId))
        {
            commandOutputBuffer.Add("Usage: arm <systemId>:<componentId>");
            return;
        }

        var response = await vehicleService.ArmAsync(vehicleId, cancellationToken);
        commandOutputBuffer.Add($"Arm result: {response.Result}");
    }

    private async Task DisarmAsync(string[] parts, CancellationToken cancellationToken)
    {
        if (!TryGetVehicleId(parts, out var vehicleId))
        {
            commandOutputBuffer.Add("Usage: disarm <systemId>:<componentId>");
            return;
        }

        var response = await vehicleService.DisarmAsync(vehicleId, cancellationToken);

        commandOutputBuffer.Add($"Disarm result: {response.Result}");
    }

    private async Task SetModeAsync(string[] parts, CancellationToken cancellationToken)
    {
        if (parts.Length < 3 || !TryParseVehicleId(parts[1], out var vehicleId) || !Enum.TryParse<VehicleMode>(parts[2], true, out var mode))
        {
            commandOutputBuffer.Add("Usage: mode <systemId>:<componentId> <mode>");
            commandOutputBuffer.Add("Example: mode 1:1 Guided");
            return;
        }

        var response = await vehicleService.SetModeAsync(
            vehicleId,
            mode,
            cancellationToken);

        commandOutputBuffer.Add($"Set mode result: {response.Result}");
    }

    private static bool TryGetVehicleId(string[] parts, out VehicleId vehicleId)
    {
        vehicleId = default;

        return parts.Length >= 2 &&
               TryParseVehicleId(parts[1], out vehicleId);
    }

    private static bool TryParseVehicleId(string value, out VehicleId vehicleId)
    {
        vehicleId = default;

        var parts = value.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        if (!byte.TryParse(parts[0], out var systemId))
        {
            return false;
        }

        if (!byte.TryParse(parts[1], out var componentId))
        {
            return false;
        }

        vehicleId = new VehicleId(systemId, componentId);
        return true;
    }

    private static string Format(VehicleId vehicleId)
    {
        return $"{vehicleId.SystemId}:{vehicleId.ComponentId}";
    }

    private void WriteHelp()
    {
        commandOutputBuffer.Add("");
        commandOutputBuffer.Add("Commands:");
        commandOutputBuffer.Add("  help");
        commandOutputBuffer.Add("  vehicles");
        commandOutputBuffer.Add("  state <systemId>:<componentId>");
        commandOutputBuffer.Add("  arm <systemId>:<componentId>");
        commandOutputBuffer.Add("  disarm <systemId>:<componentId>");
        commandOutputBuffer.Add("  mode <systemId>:<componentId> <mode>");
        commandOutputBuffer.Add("  exit");
        commandOutputBuffer.Add("");
        commandOutputBuffer.Add("Examples:");
        commandOutputBuffer.Add("  vehicles");
        commandOutputBuffer.Add("  state 1:1");
        commandOutputBuffer.Add("  arm 1:1");
        commandOutputBuffer.Add("  mode 1:1 Guided");
        commandOutputBuffer.Add("");
    }
}
