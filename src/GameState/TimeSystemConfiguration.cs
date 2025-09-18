using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for configuring the time system.
/// </summary>
public static class TimeSystemConfiguration
{
    /// <summary>
    /// Adds the time system to the service collection.
    /// </summary>
    public static IServiceCollection AddTimeSystem(this IServiceCollection services)
    {
        // Register the core time components
        services.AddSingleton<TimeModel>();

        // Register the TimeManager
        services.AddSingleton<TimeManager>();

        return services;
    }

    // Migration method removed - no longer needed
}