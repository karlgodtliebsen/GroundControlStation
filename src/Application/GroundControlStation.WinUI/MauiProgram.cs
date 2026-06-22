using GroundControlStationApp;
using GroundControlStationApp.Configuration;

namespace GroundControlStation.WinUI;

/// <summary>
/// The MauiProgram class is responsible for configuring and creating the Maui application instance. 
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the Maui application instance.
    /// </summary>
    /// <returns>The configured Maui application instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseSharedMauiApp();
        var host = builder.Build();
        host.Services.UseApplication();

        return host;
    }
}
