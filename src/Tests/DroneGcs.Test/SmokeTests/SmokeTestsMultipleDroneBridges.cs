using System.Net;
using System.Net.Sockets;

using DroneGcs.Simulator.SmokeTests;
using DroneGcs.Test.Configuration;
using DroneGcs.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DroneGcs.Test.SmokeTests;

/// <summary>
/// Tests for the domain layer implementations.
/// </summary>
public class SmokeTestsMultipleDroneBridges
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;
    private readonly IList<TransportEndpoint> endPoints = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SmokeTestsDroneBridge"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public SmokeTestsMultipleDroneBridges(ITestOutputHelper output)
    {
        this.output = output;

        var services = TestConfigurator
            .AddTestConfiguration()
            .AddDefaultTestLogging(output);

        serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseTestConfiguration();
        var localPort = 14550;
        var remotePort = 14551;
        var localHost = "0.0.0.0";

        var remoteHost1 = "192.168.1.248";
        var remoteHost2 = "192.168.1.217";

        var endPoint1 = new TransportEndpoint("udp", remotePort, remoteHost1, localPort, localHost, 512);
        var endPoint2 = new TransportEndpoint("udp", remotePort, remoteHost2, localPort, localHost, 512);

        endPoints = [endPoint1, endPoint2];


        //IOptions<TransportEndpoint[]> endPoints = serviceProvider.GetRequiredService<IOptions<TransportEndpoint[]>>();

        //for tcp: 127.0.0.1 on port 5760

        var logger = serviceProvider.GetRequiredService<ILogger<SmokeTestsMultipleDroneBridges>>();

        logger.LogInformation($"Test configuration initialized. UDP local:  {endPoint1.LocalHost}:{endPoint1.LocalPort}");
        logger.LogInformation($"Test configuration initialized. UDP remote 1: {endPoint1.RemoteHost}:{endPoint1.RemotePort}");
        logger.LogInformation($"Test configuration initialized. UDP remote 2: {endPoint2.RemoteHost}:{endPoint2.RemotePort}");
    }


    /// <summary>
    /// Sends a TCP  probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Tcp_Probe_To_DroneBridges_Without_Error()
    {
        foreach (var endPoint in endPoints)
        {
            using var tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(endPoint.RemoteHost, 5760, TestContext.Current.CancellationToken);

            Assert.True(tcpClient.Connected);
        }
    }


    /// <summary>
    /// Sends a UDP probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Factorize_Connection()
    {
        var transport = serviceProvider.GetRequiredService<IMavLinkTransport>();

        await transport.ConnectAsync(TestContext.Current.CancellationToken);

        foreach (var endPoint in endPoints)
        {
            var localPort = endPoint.LocalPort;
            var localHost = endPoint.LocalHost;
            var localAddress = string.IsNullOrWhiteSpace(localHost)
                ? IPAddress.Any
                : IPAddress.Parse(localHost);

            var ipEndPoint = new IPEndPoint(localAddress, localPort);
            var payload = TransportProbePayloads.CreateAsciiProbe();
            await transport.WriteAsync(payload, ipEndPoint, TestContext.Current.CancellationToken);
        }
    }


    //public sealed class VehicleLinkSession
    //{
    //    public VehicleId VehicleId { get; init; }

    //    public TransportEndpoint RemoteEndpoint { get; set; }

    //    public DateTimeOffset LastPacketReceivedAt { get; set; }

    //    public DateTimeOffset LastHeartbeatAt { get; set; }

    //    public VehicleConnectionState ConnectionState { get; set; }
    //}

    /// <summary>
    /// Sends a UDP probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Udp_Probe_To_DroneBridges_Without_Error()
    {
        var smokeTest = serviceProvider.GetRequiredService<ITransportSmokeTestService>();
        foreach (var endPoint in endPoints)
        {
            var localPort = endPoint.LocalPort;
            var localHost = endPoint.LocalHost;
            var localAddress = string.IsNullOrWhiteSpace(localHost)
                ? IPAddress.Any
                : IPAddress.Parse(localHost);

            var ipEndPoint = new IPEndPoint(localAddress, localPort);
            var payload = TransportProbePayloads.CreateAsciiProbe();
            await smokeTest.SendProbeAsync(payload, ipEndPoint, TestContext.Current.CancellationToken);
        }

        Assert.True(true);
    }
}
