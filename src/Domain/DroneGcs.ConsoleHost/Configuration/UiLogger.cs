using DroneGcs.ConsoleHost.Dashboard;

using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.Configuration;

public sealed class UiLogger<T>(LogBuffer buffer) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        buffer.Add($"[{DateTime.Now:HH:mm:ss}] {formatter(state, exception)}");
    }
}
