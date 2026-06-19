namespace DroneGcs.ConsoleHost.Dashboard;

/// <summary>
/// A buffer for storing monitor output lines.
/// </summary>
public sealed class MonitorOutputBuffer : ConsoleLineBuffer
{
    public MonitorOutputBuffer() : base(200)
    {
    }
}
