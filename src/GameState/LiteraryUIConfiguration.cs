using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring the Literary UI system.
/// </summary>
public static class LiteraryUIConfiguration
{
    /// <summary>
    /// Adds the Literary UI system services to the service collection.
    /// This includes attention management, NPC emotional states, and verb contextualization.
    /// </summary>
    public static IServiceCollection AddLiteraryUISystem(this IServiceCollection services)
    {
        // Register Attention Management
        services.AddSingleton<AttentionManager>();
        
        // Register NPC Emotional State Calculator
        services.AddSingleton<NPCEmotionalStateCalculator>();
        
        // Register Verb Contextualizer for hidden mechanics
        services.AddSingleton<VerbContextualizer>();

        return services;
    }
}