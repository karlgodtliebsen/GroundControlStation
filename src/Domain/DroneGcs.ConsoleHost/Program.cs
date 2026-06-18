using System.Globalization;
using DroneGcs.ConsoleHost.Configuration;
using DroneGcs.ConsoleHost.HostingLibraries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var ci = new CultureInfo("en-US");
Thread.CurrentThread.CurrentCulture = ci;
Thread.CurrentThread.CurrentUICulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;
CultureInfo.DefaultThreadCurrentCulture = ci;
const string title = "DroneGcs.Console";

HostRunner.SetAppDomainExceptionHandling(title);

Console.Title = title;
Console.WriteLine(title);
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services
            .AddConfiguration(configuration)
            .AddDefaultLogging(configuration)
            ;
    }).ConfigureHostOptions(c => { }
    )
    .Build();

host.Services.UseConfiguration();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting DroneGcs.Console application...");
await host.RunAsync();
logger.LogInformation("Stopping DroneGcs.Console application...");


namespace DroneGcs.ConsoleHost
{
    /// <summary>
    /// The main entry point for the DroneGcs.Console application.
    /// </summary>
    public partial class Program
    {
    }
}
