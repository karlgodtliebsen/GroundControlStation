namespace DroneGcs.ConsoleHost.Dashboard;

public sealed class CommandOutputBuffer : ConsoleLineBuffer
{
    public CommandOutputBuffer() : base(50)
    {
    }
}
