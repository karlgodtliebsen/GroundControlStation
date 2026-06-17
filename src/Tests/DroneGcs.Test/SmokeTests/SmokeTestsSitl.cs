using System.Net.Sockets;

using DroneGcs.Core.Models;
using DroneGcs.Core.Services;
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
public class SmokeTestsSitl
{
    private readonly ITestOutputHelper output;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmokeTestsSitl"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public SmokeTestsSitl(ITestOutputHelper output)
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
        endPoint.Value.RemoteHost = "127.0.0.1";
        endPoint.Value.RemotePort = 14551;

        endPoint.Value.LocalHost = "0.0.0.0";
        endPoint.Value.LocalPort = 14550;

        //for tcp: 127.0.0.1 on port 5760

        var logger = serviceProvider.GetRequiredService<ILogger<SmokeTestsDroneBridge>>();

        logger.LogInformation($"Test configuration initialized. UDP local:  {endPoint.Value.LocalHost}:{endPoint.Value.LocalPort}");
        logger.LogInformation($"Test configuration initialized. UDP remote: {endPoint.Value.RemoteHost}:{endPoint.Value.RemotePort}");
    }


    /// <summary>
    /// Sends a TCP  probe to the SITL without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Tcp_Probe_To_SITL_Without_Error()
    {
        using var tcpClient = new TcpClient();

        await tcpClient.ConnectAsync("127.0.0.1", 5760, TestContext.Current.CancellationToken);

        Assert.True(tcpClient.Connected);
    }

    /// <summary>
    /// Sends a UDP probe to the DroneBridge without expecting any response.
    /// </summary>
    [Fact]
    public async Task Should_Send_Udp_Probe_To_SITL_Without_Error()
    {
        var smokeTest =
            serviceProvider.GetRequiredService<ITransportSmokeTestService>();

        var payload = TransportProbePayloads.CreateAsciiProbe();

        await smokeTest.SendProbeAsync(payload, TestContext.Current.CancellationToken);

        Assert.True(true);
    }

    /*
        cat /etc/resolv.conf | grep nameserver
        nameserver 10.255.255.254

        cd ~/ardupilot/ArduCopter
        sim_vehicle.py -v ArduCopter --console --map --out=udp:10.255.255.254:14550
    */
    //Should_Receive_Heartbeat_From_SITL()

    /// <summary>
    /// Receives a MAVLink heartbeat message through the SITL.
    /// </summary>
    [Fact]
    public async Task Should_Receive_Heartbeat_From_SITL()
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


    /// <summary>
    /// Registers a vehicle from the SITL heartbeat message.
    /// </summary>
    [Fact]
    public async Task Should_Register_Vehicle_From_SITL_Heartbeat()
    {
        var logger = serviceProvider.GetRequiredService<ILogger<SmokeTestsSitl>>();
        await using var connection = serviceProvider.GetRequiredService<IMavLinkConnection>();

        var messagePump = serviceProvider.GetRequiredService<IVehicleMessagePump>();

        var vehicleService = serviceProvider.GetRequiredService<IVehicleService>();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        _ = Task.Run(
            () => messagePump.StartAsync(TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);

        await EventuallyAsync(
            () =>
            {
                var vehicles = vehicleService.GetVehicles();

                logger.LogDebug("Vehicle count: {VehicleCount}", vehicles.Count);

                Assert.NotEmpty(vehicles);

                var vehicle = vehicles.First();

                logger.LogDebug("Vehicle: {VehicleId}, State: {ConnectionState}, Mode: {Mode}", vehicle.VehicleId, vehicle.ConnectionState, vehicle.Mode);

                Assert.Equal(VehicleConnectionState.Online, vehicle.ConnectionState);
            },
            TimeSpan.FromSeconds(15),
            TestContext.Current.CancellationToken);
    }


    private static async Task EventuallyAsync(Action assertion, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay(50, cancellationToken);
            }
        }

        throw lastException ?? new TimeoutException();
    }
}
