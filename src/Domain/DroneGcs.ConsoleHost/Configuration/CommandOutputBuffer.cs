namespace DroneGcs.ConsoleHost.Configuration;

public sealed class CommandOutputBuffer : ConsoleLineBuffer
{
    public CommandOutputBuffer() : base(50)
    {
    }
}
