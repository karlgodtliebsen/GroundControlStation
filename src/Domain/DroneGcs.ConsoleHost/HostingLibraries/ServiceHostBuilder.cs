using System.Globalization;
using Domain.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroneGcs.ConsoleHost.HostingLibraries;

/// <summary>
/// Provides methods to create and configure a service host with support for application and environment specific settings.
/// </summary>
public static class ServiceHostBuilder
{
    private const string AppSettings = "appsettings";
    private const string AppSettingsExtension = "json";

    private static string GetRootAppSettingsFile()
    {
        return $"{AppSettings}.{AppSettingsExtension}";
    }

    /// <summary>
    /// Support for loading Application specific appSettings files, to support colocation (one folder multiple applications)
    /// Samples:
    /// appsettings.applicationName.json
    /// appsettings.Dispatch.json       this is the Production appsettings for the Dispatch Application
    /// </summary>
    /// <param name="name">The name of the application.</param>
    /// <returns>The path to the application-specific appsettings file.</returns>
    private static string GetAppSettingsFileUsingName(string name)
    {
        return $"{AppSettings}.{name}.{AppSettingsExtension}";
    }

    /// <summary>
    /// Support for loading Application and Environment specific appSettings files, to support colocation (one folder multiple applications)
    /// Samples:
    /// appsettings.applicationName.environment.json
    /// appsettings.Dispatch.Development.json this is the Development appsettings for the Dispatch Application
    /// </summary>
    /// <param name="environment">The environment name (e.g., Development, Production).</param>
    /// <param name="appName">The name of the application.</param>
    /// <returns>The path to the environment-specific appsettings file for the application.</returns>
    private static string GetAppSettingsFileUsingEnvironmentAndAppName(string environment, string appName)
    {
        return $"{AppSettings}.{appName}.{environment}.{AppSettingsExtension}";
    }

    private static void AddSource(string settingsFile, IList<IConfigurationSource> configurationSources, bool optional = false, bool reloadOnChange = true)
    {
        if (File.Exists(settingsFile))
        {
            IConfigurationSource source = new JsonConfigurationSource { Path = settingsFile, Optional = optional, ReloadOnChange = reloadOnChange };
            configurationSources.Add(source);
        }
    }

    /// <summary>
    /// Creates and configures a service host with support for application and environment specific settings.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="environment">The environment name (e.g., Development, Production).</param>
    /// <param name="sources">Additional configuration sources.</param>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="configureServices"></param>
    /// <param name="hostBuilder"></param>
    /// <param name="useBuilder"></param>
    /// <param name="configureOptions"></param>
    /// <param name="useAction"></param>
    /// <param name="useActionCfg"></param>
    /// <returns></returns>
    public static IHost CreateServiceHost(string? appName = null, string? environment = null, IEnumerable<IConfigurationSource>? sources = null, string[]? args = null,
        Action<HostBuilderContext, IServiceCollection, IConfiguration>? configureServices = null,
        Action<IHostBuilder, IConfiguration>? hostBuilder = null,
        Action<IHostBuilder>? useBuilder = null,
        Action<HostOptions>? configureOptions = null,
        Action<IServiceProvider>? useAction = null,
        Action<IServiceProvider, IConfiguration>? useActionCfg = null
    )
    {
        var ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
        CultureInfo.DefaultThreadCurrentCulture = ci;

        if (args is not null && args.Length > 0)
        {
            var env = args.FirstOrDefault(a => a.StartsWith("DOTNET_ENVIRONMENT"));
            if (env != null) environment = env.Split('=')[^1];
        }

        IConfiguration? configuration = null;
        IList<IConfigurationSource>? sourceCollection = [];
        IList<IConfigurationSource> configurationSources = [];
        if (sources is not null) sourceCollection = sources.ToList();

        var settingsFile = GetRootAppSettingsFile();
        AddSource(settingsFile, configurationSources);
        if (appName is not null)
        {
            settingsFile = GetAppSettingsFileUsingName(appName);
            AddSource(settingsFile, configurationSources);
        }

        if (environment is not null && appName is not null)
        {
            settingsFile = GetAppSettingsFileUsingEnvironmentAndAppName(environment, appName);
            AddSource(settingsFile, configurationSources);
        }

        var defaultBuilder = Host.CreateDefaultBuilder(args);

        if (hostBuilder is not null)
        {
            configuration = BuildToGetConfiguration(args, configurationSources, sourceCollection);
            hostBuilder?.Invoke(defaultBuilder, configuration);
        }

        defaultBuilder
            .ConfigureHostConfiguration(configurationBuilder => AddConfigurationSources(configurationSources, configurationBuilder, sourceCollection))
            .ConfigureServices((context, services) => { configureServices?.Invoke(context, services, context.Configuration!); })
            .ConfigureHostOptions(hostOptions => { configureOptions?.Invoke(hostOptions); })
            .UseConsoleLifetime();
        useBuilder?.Invoke(defaultBuilder);
        var host = defaultBuilder.Build();
        useAction?.Invoke(host.Services);
        if (configuration is not null) useActionCfg?.Invoke(host.Services, configuration);

        return host;
    }

    private static void AddConfigurationSources(IList<IConfigurationSource> configurationSources, IConfigurationBuilder configurationBuilder, IList<IConfigurationSource>? sourceCollection)
    {
        foreach (var source in configurationSources) configurationBuilder.Sources.Add(source);

        foreach (var source in sourceCollection) configurationBuilder.Sources.Add(source);
    }

    private static IConfiguration BuildToGetConfiguration(string[]? args, IList<IConfigurationSource> configurationSources, IEnumerable<IConfigurationSource>? sources = null)
    {
        IConfiguration? configuration = null;
        var tempBuilder = Host.CreateDefaultBuilder(args);
        tempBuilder.ConfigureHostConfiguration(configurationBuilder =>
        {
            foreach (var source in configurationSources) configurationBuilder.Sources.Add(source);

            if (sources is not null)
                foreach (var source in sources)
                    configurationBuilder.Sources.Add(source);
        }).ConfigureServices((context, _) => { configuration = context.Configuration; });

        tempBuilder.Build();
        DomainException.ThrowIfNull(configuration);
        return configuration;
    }
}
