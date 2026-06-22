using GroundControlStationApp;
using GroundControlStationApp.Configuration;

namespace GroundControlStation.iOS;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseSharedMauiApp();

        var host = builder.Build();
        host.Services.UseApplication();

        return host;
    }
}
