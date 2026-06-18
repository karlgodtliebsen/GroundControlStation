namespace DroneGcs.ConsoleHost.Configuration;

public sealed class LoggingOutputBuffer : ConsoleLineBuffer
{
    public LoggingOutputBuffer() : base(200)
    {
    }
}
