using System.Net;
using System.Net.Sockets;

using Domain.Library.EventHub.Abstractions;

using DroneGcs.Simulator.SmokeTests;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using DroneGs.MavLink;
using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneGcs.Test.SmokeTests;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class SmokeTestsDroneBridge
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;
    private readonly IPEndPoint ipEndPoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmokeTestsDroneBridge"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public SmokeTestsDroneBridge(ITestOutputHelper output)
    {
        this.output = output;

        var services = TestConfigurator
            .AddTestConfiguration()
            .AddDefaultTestLogging(output);

        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();

        var endPoint = serviceProvider.GetRequiredService<IOptions<TransportEndpoint>>().Value;
        endPoint.LocalPort = 14550;
        endPoint.RemotePort = 14551;

        endPoint.LocalHost = "0.0.0.0";
        endPoint.RemoteHost = "192.168.1.248";

        //for tcp: 127.0.0.1 on port 5760
        var localPort = endPoint.LocalPort;
        var localHost = endPoint.LocalHost;
        var localAddress = string.IsNullOrWhiteSpace(localHost)
            ? IPAddress.Any
            : IPAddress.Parse(localHost);
        ipEndPoint = new IPEndPoint(localAddress, localPort);

        var logger = serviceProvider.GetRequiredService<ILogger<SmokeTestsDroneBridge>>();

        logger.LogInformation($"Test configuration initialized. UDP local:  {endPoint.LocalHost}:{endPoint.LocalPort}");
        logger.LogInformation($"Test configuration initialized. UDP remote: {endPoint.RemoteHost}:{endPoint.RemotePort}");
    }


    /// <summary>
    /// Sends a TCP  probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Tcp_Probe_To_DroneBridge_Without_Error()
    {
        using var tcpClient = new TcpClient();

        await tcpClient.ConnectAsync("192.168.1.248", 5760, TestContext.Current.CancellationToken);

        Assert.True(tcpClient.Connected);
    }


    /// <summary>
    /// Sends a UDP probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Udp_Probe_To_DroneBridge_Without_Error()
    {
        var smokeTest =
            serviceProvider.GetRequiredService<ITransportSmokeTestService>();

        var payload = TransportProbePayloads.CreateAsciiProbe();

        await smokeTest.SendProbeAsync(payload, ipEndPoint, TestContext.Current.CancellationToken);

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

        await smokeTest.SendProbeAsync(payload, ipEndPoint, TestContext.Current.CancellationToken);

        // This will only pass if something on the DroneBridge side sends data back.
        var result = await smokeTest.WaitForDataAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.True(result.BytesReceived > 0);
    }

    /// <summary>
    /// Receives a MAVLink heartbeat message through the DroneBridge.
    /// </summary>
    [Fact]
    public async Task Should_Receive_MavLink_Heartbeat_Through_DroneBridge()
    {
        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        await using var connection = serviceProvider.GetRequiredService<IMavLinkConnection>();

        await connection.StartAsync(TestContext.Current.CancellationToken);
        TaskCompletionSource ts = new(TaskCreationOptions.RunContinuationsAsynchronously);
        MavLinkFrame? messageResult = null;
        using var subscription = eventHub.SubscribeAsync<MavLinkFrame>(MavLinkEventTopics.ReceivedFrame, (frame, cts) =>
        {
            if (frame.MessageId == MessageIds.Heartbeat)
            {
                messageResult = frame;
                ts.TrySetResult();
            }

            return Task.CompletedTask;
        });

        await ts.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        messageResult.Should().NotBeNull();
        var frame = messageResult!;
        Assert.Equal(MessageIds.Heartbeat, frame.MessageId);
        Assert.True(frame.SystemId > 0);
    }
}
