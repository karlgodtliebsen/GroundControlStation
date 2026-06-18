using Microsoft.Extensions.Logging;

namespace DroneGcs.ConsoleHost.Configuration;

public sealed class BufferedLogger(
    string categoryName,
    LoggingOutputBuffer buffer)
    : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);

        if (string.IsNullOrWhiteSpace(message)) return;

        var category = ShortenCategory(categoryName);

        buffer.Add(
            $"[{DateTimeOffset.Now:HH:mm:ss}] {logLevel,-11} {category}: {message}");

        if (exception is not null) buffer.Add(exception.ToString());
    }

    private static string ShortenCategory(string category)
    {
        var lastDot = category.LastIndexOf('.');

        return lastDot >= 0 && lastDot < category.Length - 1
            ? category[(lastDot + 1)..]
            : category;
    }
}
