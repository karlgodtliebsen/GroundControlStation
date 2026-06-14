using DroneGcs.Transport;

namespace DroneGcs.Test;

/// <summary>
/// Tests for the MAVLink client and transport implementations.
/// </summary>
public class MavLinkTests
{
    //UDP transport
    //Receive loop
    //Events
    //Cancellation
    //Lifecycle


    /// <summary>
    /// Tests that the MAVLink client can receive data from a fake vehicle.
    /// </summary>
    [Fact]
    public async Task Should_Receive_Data_From_Fake_Vehicle()
    {
        var transport = new UdpMavLinkTransport(14550, "127.0.0.1", 14551);
        var client = new MavLinkClient(transport);
        var received = new TaskCompletionSource<byte[]>();

        client.DataReceived += (data, _) =>
        {
            received.TrySetResult(data.Data.ToArray());

            return ValueTask.CompletedTask;
        };

        await client.StartAsync(TestContext.Current.CancellationToken);

        await using FakeMavLinkVehicle simulator = new("127.0.0.1", 14550);

        await simulator.StartAsync();

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotEmpty(result);

        await client.StopAsync();
    }
}

//DroneGcs.Core.Tests
//    ↓
//FakeMavLinkVehicle
//    ↓ UDP loopback
//DroneGcs.MavLink
