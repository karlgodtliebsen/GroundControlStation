using CommunityToolkit.Maui;

using Microsoft.Extensions.Logging;

using Mopups.Hosting;

using UraniumUI;

namespace GroundControlStationApp;

/// <summary>
/// Extension methods for configuring the MauiAppBuilder.
/// </summary>
public static class MauiProgramExtensions
{
    /// <summary>
    /// Configures the MauiAppBuilder with shared settings and services.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance to configure.</param>
    /// <returns>The configured MauiAppBuilder instance.</returns>
    public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureMopups()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFontAwesomeIconFonts();
            });
        builder.Services.AddCommunityToolkitDialogs();
        builder.Services.AddMopupsDialogs();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }
}