namespace DroneGcs.ConsoleHost.Dashboard;

public sealed class LoggingOutputBuffer : ConsoleLineBuffer
{
    public LoggingOutputBuffer() : base(200)
    {
    }
}
