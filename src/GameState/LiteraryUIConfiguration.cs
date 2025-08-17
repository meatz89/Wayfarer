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
        
        // Register Atmosphere Calculator for NPC-based environmental effects
        services.AddSingleton<AtmosphereCalculator>();
        
        // Register NPC State Resolver
        services.AddSingleton<NPCStateResolver>();
        
        // Card-based conversation system - no verb contextualizer needed
        
        // Register Environmental Systems
        services.AddSingleton<EnvironmentalHintSystem>();
        services.AddSingleton<ObservationSystem>();
        
        // Register Action Beat Generator for mid-dialogue atmosphere
        services.AddSingleton<ActionBeatGenerator>();
        
        // Register Binding Obligation System for tracking promises and debts
        services.AddSingleton<BindingObligationSystem>();

        return services;
    }
}