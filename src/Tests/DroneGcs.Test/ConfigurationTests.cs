using Domain.Library.EventHub;
using Domain.Library.Factory.Domain.Abstractions;

using DroneGcs.Core.Commands;
using DroneGcs.Core.Services;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink.Client;
using DroneGs.MavLink.Encoding;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class ConfigurationTests
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public ConfigurationTests(ITestOutputHelper output)
    {
        this.output = output;
        var logger = NSubstitute.Substitute.For<ILogger<EventHub>>();

        var services = TestConfigurator
            .AddTestConfiguration();
        services.AddSingleton<ILogger<EventHub>>(logger);
        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();
    }


    [Fact]
    public async Task TestConfigurationSetupAsync()
    {
        var parser = serviceProvider.GetRequiredService<IMavLinkFrameParser>();
        var transport = serviceProvider.GetRequiredService<IMavLinkTransport>();
        var mavLinkClient = serviceProvider.GetRequiredService<IMavLinkClient>();

        var frameParser = serviceProvider.GetRequiredService<IMavLinkFrameParser>();
        var commandEncoder = serviceProvider.GetRequiredService<IMavLinkCommandEncoder>();
        var crcExtraProvider = serviceProvider.GetRequiredService<IMavLinkCrcExtraProvider>();
        var messageDecoder = serviceProvider.GetRequiredService<IMavLinkMessageDecoder>();

        var connection = serviceProvider.GetRequiredService<IMavLinkConnection>();
        var messagePump = serviceProvider.GetRequiredService<IVehicleMessagePump>();
        var commandAckTracker = serviceProvider.GetRequiredService<ICommandAckTracker>();
        var commandService = serviceProvider.GetRequiredService<IVehicleCommandService>();
        var registry = serviceProvider.GetRequiredService<IVehicleRegistry>();
        var vehicleService = serviceProvider.GetRequiredService<IVehicleService>();

        var domainFactory = serviceProvider.GetRequiredService<IDomainFactory>();

        await using var simulator = new FakeMavLinkVehicle2(frameParser, crcExtraProvider, "127.0.0.1", 14550, 14551, TimeSpan.FromMilliseconds(100));
    }
}
