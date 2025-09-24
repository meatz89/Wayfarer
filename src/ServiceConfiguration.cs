using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        Console.WriteLine("[SERVICE] Starting service configuration...");

        // Register dev mode service (needs to be early for other services to use)
        services.AddSingleton<DevModeService>();

        // Register configuration
        services.AddSingleton<IContentDirectory, ContentDirectory>();

        // Register game configuration and rule engine
        services.AddSingleton<GameConfiguration>();
        services.AddSingleton<IGameRuleEngine, GameRuleEngine>();


        // Register GameWorld using static GameWorldInitializer
        Console.WriteLine("[SERVICE] Registering GameWorld...");
        services.AddSingleton<GameWorld>(_ =>
        {
            Console.WriteLine("[SERVICE] Creating GameWorld instance...");
            // Call GameWorldInitializer statically - no DI dependencies needed
            GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
            Console.WriteLine("[SERVICE] GameWorld instance created");
            return gameWorld;
        });
        Console.WriteLine("[SERVICE] GameWorld registered");

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Core services that have no dependencies
        services.AddSingleton<NPCVisibilityService>();

        // Register repositories
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<StandingObligationRepository>();
        services.AddSingleton<RouteDiscoveryRepository>();

        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        services.AddTimeSystem();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelManager>();
        services.AddSingleton<TransportCompatibilityValidator>();

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();


        // ConversationSubsystem services
        services.AddSingleton<FlowManager>();
        services.AddSingleton<MomentumManager>();
        services.AddSingleton<CategoricalEffectResolver>();
        services.AddSingleton<ConversationDeckBuilder>();
        services.AddSingleton<ExchangeHandler>();
        services.AddSingleton<ConversationFacade>();

        // NPC deck initialization handled directly in PackageLoader

        // Dialogue generation services (NO hardcoded text)
        services.AddSingleton<DialogueGenerationService>();

        // Narrative Generation System (AI and JSON fallback)
        // Infrastructure for Ollama  
        services.AddHttpClient<OllamaClient>(client =>
        {
            // Short timeout to prevent hanging when Ollama unavailable
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddSingleton<OllamaConfiguration>();

        // Narrative Generation Services
        services.AddSingleton<NarrativeStreamingService>();
        services.AddSingleton<JsonNarrativeRepository>();
        services.AddSingleton<ConversationNarrativeService>();

        // AI Narrative Generation Support Components
        services.AddSingleton<ConversationNarrativeGenerator>();
        services.AddSingleton<PromptBuilder>();

        // Always register both providers as concrete types
        services.AddSingleton<JsonNarrativeProvider>();
        services.AddSingleton<AIConversationNarrativeProvider>();

        // NarrativeProviderFactory will handle selection based on config
        services.AddSingleton<NarrativeProviderFactory>();

        // Wire up circular dependencies after initial creation
        services.AddSingleton<TokenMechanicsManager>();

        // Observation management system
        services.AddSingleton<ObservationManager>();

        // Environmental Storytelling Systems
        services.AddSingleton<ObservationSystem>();
        services.AddSingleton<BindingObligationSystem>();

        services.AddSingleton<ObligationQueueManager>();

        // Transaction and Preview System
        services.AddSingleton<AccessRequirementChecker>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<SpecialLetterHandler>();
        services.AddSingleton<DailyActivitiesManager>();


        // Action generation service
        services.AddSingleton<ActionGenerator>();

        // Core services
        services.AddScoped<MusicService>();
        services.AddScoped<TimeImpactCalculator>();

        // Location Subsystem
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationManager>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationSpotManager>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.MovementValidator>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.NPCLocationTracker>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationActionManager>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationNarrativeGenerator>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationFacade>();

        // Obligation Subsystem
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.DeliveryManager>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.MeetingManager>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.QueueManipulator>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.DisplacementCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.DeadlineTracker>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.ObligationStatistics>();
        services.AddSingleton<Wayfarer.Subsystems.ObligationSubsystem.ObligationFacade>();

        // Resource Subsystem
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.CoinManager>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.HealthManager>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.HungerManager>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.ResourceCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.ResourceFacade>();

        // Time Subsystem
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeBlockCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeProgressionManager>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeDisplayFormatter>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeFacade>();

        // Travel Subsystem
        services.AddSingleton<Wayfarer.Subsystems.TravelSubsystem.RouteManager>();
        services.AddSingleton<Wayfarer.Subsystems.TravelSubsystem.RouteDiscoveryManager>();
        services.AddSingleton<Wayfarer.Subsystems.TravelSubsystem.PermitValidator>();
        services.AddSingleton<Wayfarer.Subsystems.TravelSubsystem.TravelTimeCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.TravelSubsystem.TravelFacade>();

        // Market Subsystem
        services.AddSingleton<Wayfarer.Subsystems.MarketSubsystem.MarketSubsystemManager>();
        services.AddSingleton<Wayfarer.Subsystems.MarketSubsystem.PriceManager>();
        services.AddSingleton<Wayfarer.Subsystems.MarketSubsystem.ArbitrageCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.MarketSubsystem.MarketStateTracker>();
        services.AddSingleton<Wayfarer.Subsystems.MarketSubsystem.MarketFacade>();

        // Exchange Subsystem
        services.AddSingleton<Wayfarer.Subsystems.ExchangeSubsystem.ExchangeValidator>();
        services.AddSingleton<Wayfarer.Subsystems.ExchangeSubsystem.ExchangeProcessor>();
        services.AddSingleton<Wayfarer.Subsystems.ExchangeSubsystem.ExchangeInventory>();
        services.AddSingleton<Wayfarer.Subsystems.ExchangeSubsystem.ExchangeOrchestrator>();
        services.AddSingleton<Wayfarer.Subsystems.ExchangeSubsystem.ExchangeFacade>();

        // Token Subsystem
        services.AddSingleton<Wayfarer.Subsystems.TokenSubsystem.ConnectionTokenManager>();
        services.AddSingleton<Wayfarer.Subsystems.TokenSubsystem.TokenEffectProcessor>();
        services.AddSingleton<Wayfarer.Subsystems.TokenSubsystem.TokenUnlockManager>();
        services.AddSingleton<Wayfarer.Subsystems.TokenSubsystem.RelationshipTracker>();
        services.AddSingleton<Wayfarer.Subsystems.TokenSubsystem.TokenFacade>();

        // Narrative Subsystem
        services.AddSingleton<Wayfarer.Subsystems.NarrativeSubsystem.ObservationManagerWrapper>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<Wayfarer.Subsystems.NarrativeSubsystem.NarrativeRenderer>();
        services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationNarrativeGenerator>();
        services.AddSingleton<Wayfarer.Subsystems.NarrativeSubsystem.NarrativeFacade>();

        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();


        return services;
    }


}