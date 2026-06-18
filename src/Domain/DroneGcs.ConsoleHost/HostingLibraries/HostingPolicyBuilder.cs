using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace DroneGcs.ConsoleHost.HostingLibraries;

/// <summary>
/// Provides methods to create Polly retry policies for hosting services.   
/// </summary>
public static class HostingPolicyBuilder
{
    /// <summary>
    /// Creates a Polly retry policy for a hosting service with an initial retry policy followed by a continuous retry policy.
    /// </summary>
    /// <param name="serviceName">The name of the service for logging purposes.</param>
    /// <param name="continuousRetryTimeSpan">The time span between continuous retries.</param>
    /// <param name="logger">The logger to use for logging retry attempts.</param>
    /// <returns>An AsyncPolicyWrap combining the initial and continuous retry policies.</returns>
    public static AsyncPolicyWrap CreateRetryPolicy(string serviceName, TimeSpan continuousRetryTimeSpan, ILogger logger)
    {
        // Initial retry policy: 3 attempts with exponential backoff
        var initialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                    logger.LogWarning(exception, "{serviceName} - Initial retry {retryCount} after {timeSpan}",
                        serviceName, retryCount, timeSpan));

        // Continuous retry policy: retry every minute indefinitely
        var continuousRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(
                retryAttempt => continuousRetryTimeSpan,
                (exception, retryCount, timeSpan) =>
                    logger.LogWarning(exception, "{serviceName} - Continuous retry {retryCount} after {timeSpan}",
                        serviceName, retryCount, timeSpan));

        // Combine the policies: initial retries first, then continuous retries
        return Policy.WrapAsync(continuousRetryPolicy, initialRetryPolicy);
    }
}
