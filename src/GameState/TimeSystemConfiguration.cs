using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wayfarer.Core.Repositories;
using Wayfarer.GameState.DayTransitionHandlers;

namespace Wayfarer.GameState;

/// <summary>
/// Extension methods for configuring the unified time system.
/// </summary>
public static class TimeSystemConfiguration
{
    /// <summary>
    /// Adds the unified time system to the service collection.
    /// </summary>
    public static IServiceCollection AddUnifiedTimeSystem(this IServiceCollection services)
    {
        // Register the core time components
        services.AddSingleton<TimeModel>(sp =>
        {
            // Initialize with game start time
            return new TimeModel(startDay: 1, startHour: TimeModel.ACTIVE_DAY_START);
        });

        // Register day transition handlers
        services.AddTransient<IDayTransitionHandler, LetterQueueHandler>();
        services.AddTransient<IDayTransitionHandler, MorningActivitiesHandler>();
        services.AddTransient<IDayTransitionHandler, SaveGameHandler>();

        // Register the orchestrator
        services.AddSingleton<DayTransitionOrchestrator>(sp =>
        {
            var handlers = sp.GetServices<IDayTransitionHandler>();
            var timeModel = sp.GetRequiredService<TimeModel>();
            var logger = sp.GetRequiredService<ILogger<DayTransitionOrchestrator>>();
            
            return new DayTransitionOrchestrator(handlers, timeModel, logger);
        });

        // Register the unified time service
        services.AddSingleton<UnifiedTimeService>();

        // Register the adapter for backward compatibility
        services.AddSingleton<ITimeManager>(sp =>
        {
            var unifiedService = sp.GetRequiredService<UnifiedTimeService>();
            var player = sp.GetRequiredService<Player>();
            var worldState = sp.GetRequiredService<WorldState>();
            
            return new TimeManagerRefactoringAdapter(unifiedService, player, worldState);
        });

        return services;
    }

    /// <summary>
    /// Migrates from the old TimeManager to the unified time system.
    /// </summary>
    public static void MigrateToUnifiedTimeSystem(
        this GameWorld gameWorld,
        UnifiedTimeService unifiedTimeService)
    {
        // Get current time from old system
        var currentHour = gameWorld.TimeManager?.CurrentTimeHours ?? TimeModel.ACTIVE_DAY_START;
        var currentDay = gameWorld.WorldState?.CurrentDay ?? 1;

        // Create new time model with current state
        var timeModel = new TimeModel(currentDay, currentHour);
        
        // Replace the time manager
        var adapter = new TimeManagerRefactoringAdapter(
            unifiedTimeService, 
            gameWorld.GetPlayer(), 
            gameWorld.WorldState);
        
        // Update GameWorld to use the adapter
        // This would require making TimeManager settable or using reflection
        // For now, this is a conceptual example
    }
}