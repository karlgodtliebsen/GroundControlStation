using System.Net.Sockets;

using DroneGcs.Simulator.SmokeTests;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneGcs.Test.SmokeTests;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class SmokeTests
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmokeTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public SmokeTests(ITestOutputHelper output)
    {
        this.output = output;

        var services = TestConfigurator
            .AddTestConfiguration()
            .AddDefaultTestLogging(output);


        //services.Configure<TransportEndpoint>(options =>
        //{
        //    options.Protocol = "udp";
        //    options.LocalHost = "0.0.0.0";
        //    options.LocalPort = 14550;

        //    options.RemoteHost = "192.168.1.248";
        //    options.RemotePort = 14551;
        //});


        //services.Configure<TransportEndpoint>(options =>
        //{
        //    options.Protocol = "udp";
        //    options.LocalHost = "127.0.0.1";
        //    options.LocalPort = 14550;

        //    options.RemoteHost = "127.0.0.1";
        //    options.RemotePort = 14551;
        //});


        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();

        var endPoint = serviceProvider.GetRequiredService<IOptions<TransportEndpoint>>();
        endPoint.Value.LocalPort = 14550;
        endPoint.Value.RemotePort = 14551;

        endPoint.Value.LocalHost = "0.0.0.0";
        endPoint.Value.RemoteHost = "192.168.1.248";

        var logger = serviceProvider.GetRequiredService<ILogger<SmokeTests>>();

        logger.LogInformation($"Test configuration initialized. UDP local:  {endPoint.Value.LocalHost}:{endPoint.Value.LocalPort}");
        logger.LogInformation($"Test configuration initialized. UDP remote: {endPoint.Value.RemoteHost}:{endPoint.Value.RemotePort}");
    }


    /// <summary>
    /// Sends a UDP probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Tcp_Probe_To_DroneBridge_Without_Error()
    {
        using var tcpClient = new TcpClient();

        await tcpClient.ConnectAsync("192.168.1.248", 5760, TestContext.Current.CancellationToken);

        Assert.True(tcpClient.Connected);
    }


    [Fact]
    public async Task Should_Send_Udp_Probe_To_DroneBridge_Without_Error()
    {
        var smokeTest =
            serviceProvider.GetRequiredService<ITransportSmokeTestService>();

        var payload = TransportProbePayloads.CreateAsciiProbe();

        await smokeTest.SendProbeAsync(
            payload,
            TestContext.Current.CancellationToken);

        Assert.True(true);
    }

    /// <summary>
    /// Sends a UDP probe to the DroneBridge and verifies that data is received.
    /// </summary>
    [Fact]
    public async Task Should_Send_Udp_Probe_To_DroneBridge()
    {
        var smokeTest = serviceProvider.GetRequiredService<ITransportSmokeTestService>();

        var payload = TransportProbePayloads.CreateAsciiProbe();

        await smokeTest.SendProbeAsync(payload, TestContext.Current.CancellationToken);

        // This will only pass if something on the DroneBridge side sends data back.
        var result = await smokeTest.WaitForDataAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.True(result.BytesReceived > 0);
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public async Task Should_Receive_MavLink_Heartbeat_Through_DroneBridge()
    {
        await using var connection = serviceProvider.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        var frame = await connection
            .ReadFramesAsync(TestContext.Current.CancellationToken)
            .FirstAsync(
                frame => frame.MessageId == MessageIds.Heartbeat,
                TestContext.Current.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(15),
                TestContext.Current.CancellationToken);

        Assert.Equal(MessageIds.Heartbeat, frame.MessageId);
        Assert.True(frame.SystemId > 0);
    }
}
