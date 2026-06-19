using DroneGcs.Core.Services;

using Microsoft.Extensions.Hosting;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace DroneGcs.ConsoleHost.Dashboard;

/// <summary>
/// A background service that displays a dashboard with vehicle and log information.
/// </summary>
/// <param name="vehicleService">The service providing vehicle information.</param>
/// <param name="loggingOutputBuffer">The buffer containing log entries.</param>
/// <param name="commandOutputBuffer">The buffer containing command output.</param>
/// <param name="monitorOutputBuffer">The buffer containing monitor output.</param>
public sealed class DashboardService(IVehicleService vehicleService, LoggingOutputBuffer loggingOutputBuffer, CommandOutputBuffer commandOutputBuffer, MonitorOutputBuffer monitorOutputBuffer) : BackgroundService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await AnsiConsole.Live(new Panel("Starting...")).StartAsync(async ctx =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ctx.UpdateTarget(BuildDashboard());

                await Task.Delay(500, stoppingToken);
            }
        });
    }


    private Table BuildLogTable()
    {
        var table = new Table()
            .NoBorder()
            .HideHeaders();

        table.AddColumn("Line");

        foreach (var line in loggingOutputBuffer.GetLines().TakeLast(20))
        {
            table.AddRow(Markup.Escape(line));
        }

        return table;
    }


    private IRenderable BuildDashboard()
    {
        var grid = new Grid();

        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow(
            BuildVehiclePanel(),
            BuildCommandOutputPanel(),
            BuildDomainEventPanel()
        );

        grid.AddRow(
            new Panel(BuildLogTable())
            {
                Header = new PanelHeader("Logs")
            },
            new Panel("Type commands below the dashboard")
            {
                Header = new PanelHeader("Input")
            });

        return grid;
    }

    private Panel BuildCommandOutputPanel()
    {
        var text = string.Join(Environment.NewLine, commandOutputBuffer.GetLines().TakeLast(15));

        return new Panel(Markup.Escape(text))
        {
            Header = new PanelHeader("Command Output")
        };
    }

    private Panel BuildDomainEventPanel()
    {
        var text = string.Join(Environment.NewLine, monitorOutputBuffer.GetLines().TakeLast(15));

        return new Panel(Markup.Escape(text))
        {
            Header = new PanelHeader("Domain Events")
        };
    }

    private Panel BuildVehiclePanel()
    {
        var table = new Table();

        table.AddColumn("Vehicle");
        table.AddColumn("Mode");
        table.AddColumn("Armed");
        table.AddColumn("Battery");

        foreach (var vehicle in vehicleService.GetVehicles())
        {
            table.AddRow($"{vehicle.VehicleId.SystemId}:{vehicle.VehicleId.ComponentId}", vehicle.Mode.ToString(), vehicle.IsArmed.ToString(), $"{vehicle.BatteryRemaining}%");
        }

        return new Panel(table)
        {
            Header = new PanelHeader("Vehicles")
        };
    }
}
