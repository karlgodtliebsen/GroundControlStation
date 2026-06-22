using CommunityToolkit.Maui;

using GroundControlStationApp.Configuration;

using Microsoft.Extensions.Configuration;
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
            .UseUraniumUIBlurs(false)
            .ConfigureMopups()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFontAwesomeIconFonts();
                //fonts.AddMaterialSymbolsFonts();
                //fonts.AddFluentIconFonts();
            });

        IConfiguration configuration = builder.Configuration;

        builder.Services.AddApplicationConfiguration(configuration);

        //var thisAssembly = typeof(MauiProgram).Assembly;

        //builder.Services.AddServicesFrom(
        //        type => typeof(Page).IsAssignableFrom(type),
        //        ServiceLifetime.Transient,
        //        options => options.Assembly = thisAssembly)
        //    .AddServicesByAttributes(assembly: thisAssembly);


        builder.Services.AddCommunityToolkitDialogs();
        builder.Services.AddMopupsDialogs();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }
}
