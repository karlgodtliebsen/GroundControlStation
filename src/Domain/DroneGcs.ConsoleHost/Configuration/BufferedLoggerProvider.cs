using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.Configuration;

public sealed class BufferedLoggerProvider(
    LoggingOutputBuffer buffer)
    : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new BufferedLogger(
            categoryName,
            buffer);
    }

    public void Dispose()
    {
    }
}
