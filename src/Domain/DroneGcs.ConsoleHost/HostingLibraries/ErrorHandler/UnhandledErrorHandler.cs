using System.Diagnostics;

namespace DroneGcs.ConsoleHost.HostingLibraries.ErrorHandler;

/// <summary>
/// Provides methods to handle unhandled exceptions and unobserved task exceptions.
/// </summary>
public static class UnhandledErrorHandler
{
    private static Action<Exception, string>? logger;

    /// <summary>
    /// Sets the error handling mechanism for unhandled exceptions and unobserved task exceptions.
    /// </summary>
    /// <param name="log">The logging action to be used for reporting exceptions.</param>
    public static void SetErrorHandling(Action<Exception, string> log) //deferred logger
    {
        logger = log;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine($"Unobserved exception: {e.Exception.Message}");
        e.SetObserved();
        Debug.Print("Unhandled Exception");
        Debug.Print(e.Exception.ToString());
        Trace.WriteLine("Unhandled Exception");
        Trace.WriteLine(e.Exception.ToString());
        if (logger is not null)
        {
            var ex = e.Exception as Exception ?? new Exception();
            logger(ex, "Unobserved Exception");
        }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine("Unhandled Exception");
        Console.WriteLine(e.ExceptionObject);
        Debug.Print("Unhandled Exception");
        Debug.Print(e.ExceptionObject.ToString());
        Trace.WriteLine("Unhandled Exception");
        Trace.WriteLine(e.ExceptionObject.ToString());
        if (logger is not null)
        {
            var ex = e.ExceptionObject as Exception ?? new Exception();
            logger(ex, "Exception");
        }
    }
}
