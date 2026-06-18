using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace DroneGcs.ConsoleHost.HostingLibraries;

/// <summary>
/// Provides methods to run hosts and handle unhandled exceptions.
/// </summary>
public static class HostRunner
{
    /// <summary>
    /// Sets the error handling mechanism for unhandled exceptions in the current application domain.
    /// </summary>
    /// <param name="title">The title used for logging purposes.</param>
    public static void SetAppDomainExceptionHandling(string title)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, ex) => { CurrentDomainUnhandledException(title, ex); };
    }

    /// <summary>
    /// Handles unhandled exceptions in the current application domain by logging the exception details to the console and debug output. 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="e"></param>
    public static void CurrentDomainUnhandledException(string title, UnhandledExceptionEventArgs e)
    {
        // ReSharper disable once LocalizableElement
        Console.WriteLine($"{title} Unhandled Exception");
        Console.WriteLine(e.ExceptionObject);
        Debug.Print($"{title} Unhandled Exception");
        Debug.Print(e.ExceptionObject.ToString());
        Trace.WriteLine($"{title} Unhandled Exception");
        Trace.WriteLine(e.ExceptionObject.ToString());
    }


    /// <summary>
    /// Runs a single host asynchronously and handles any unhandled exceptions.
    /// </summary>
    /// <param name="host">The host to run.</param>
    /// <param name="title">The title used for logging purposes.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunHostAsync(IHost host, string title, CancellationToken cancellationToken)
    {
        try
        {
            return host.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.Print($"{title} Unhandled Exception");
            Debug.Print(ex.ToString());
            Trace.WriteLine($"{title} Unhandled Exception");
            Trace.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Runs multiple hosts asynchronously and handles any unhandled exceptions.
    /// </summary>
    /// <param name="hosts">The collection of hosts to run.</param>
    /// <param name="title">The title used for logging purposes.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunHostsAsync(IEnumerable<IHost> hosts, string title, CancellationToken cancellationToken)
    {
        try
        {
            IList<Task> tasks = [];
            foreach (var host in hosts) tasks.Add(host.RunAsync(cancellationToken));

            return Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Debug.Print($"{title} Unhandled Exception");
            Debug.Print(ex.ToString());
            Trace.WriteLine($"{title} Unhandled Exception");
            Trace.WriteLine(ex.ToString());
            throw;
        }
    }
}
