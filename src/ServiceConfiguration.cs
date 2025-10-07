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

        // V3 Card-Based Investigation System - DELETED (wrong architecture)
        // Investigation is strategic activity, not tactical system
        // Mental/Physical facades will be added in refactor
        services.AddSingleton<TravelObstacleService>();
        services.AddSingleton<ObstacleFacade>();

        services.AddTimeSystem();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelManager>();
        services.AddSingleton<TransportCompatibilityValidator>();

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();


        // ConversationSubsystem services
        services.AddSingleton<MomentumManager>();
        services.AddSingleton<SocialEffectResolver>();
        services.AddSingleton<SocialChallengeDeckBuilder>();
        services.AddSingleton<ExchangeHandler>();
        services.AddSingleton<SocialFacade>();
        services.AddSingleton<MentalFacade>();
        services.AddSingleton<PhysicalFacade>();

        // Player exertion calculator for dynamic cost modifiers (required by Mental/Physical effect resolvers)
        services.AddSingleton<PlayerExertionCalculator>();

        // Mental services
        services.AddSingleton<MentalEffectResolver>();
        services.AddSingleton<MentalNarrativeService>();
        services.AddSingleton<MentalDeckBuilder>();

        // Physical services
        services.AddSingleton<PhysicalEffectResolver>();
        services.AddSingleton<PhysicalNarrativeService>();
        services.AddSingleton<PhysicalDeckBuilder>();

        // Investigation Activity - Strategic orchestrator for multi-phase investigations
        services.AddSingleton<InvestigationActivity>();
        services.AddSingleton<InvestigationDiscoveryEvaluator>();
        services.AddSingleton<KnowledgeService>();

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
        services.AddSingleton<SocialNarrativeService>();

        // AI Narrative Generation Support Components
        services.AddSingleton<SocialNarrativeGenerator>();
        services.AddSingleton<PromptBuilder>();

        // Always register both providers as concrete types
        services.AddSingleton<JsonNarrativeProvider>();
        services.AddSingleton<AINarrativeProvider>();

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
        services.AddSingleton<LocationManager>();
        services.AddSingleton<LocationSpotManager>();
        services.AddSingleton<MovementValidator>();
        services.AddSingleton<NPCLocationTracker>();
        services.AddSingleton<LocationActionManager>();
        services.AddSingleton<LocationNarrativeGenerator>();
        services.AddSingleton<LocationFacade>();

        // Obligation Subsystem
        services.AddSingleton<DeliveryManager>();
        services.AddSingleton<MeetingManager>();
        services.AddSingleton<QueueManipulator>();
        services.AddSingleton<DisplacementCalculator>();
        services.AddSingleton<DeadlineTracker>();
        services.AddSingleton<ObligationStatistics>();
        services.AddSingleton<ObligationFacade>();

        // Resource Subsystem
        services.AddSingleton<CoinManager>();
        services.AddSingleton<HealthManager>();
        services.AddSingleton<HungerManager>();
        services.AddSingleton<ResourceCalculator>();
        services.AddSingleton<ResourceFacade>();

        // Time Subsystem
        services.AddSingleton<TimeBlockCalculator>();
        services.AddSingleton<TimeProgressionManager>();
        services.AddSingleton<TimeDisplayFormatter>();
        services.AddSingleton<TimeFacade>();

        // Travel Subsystem
        services.AddSingleton<RouteManager>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<PermitValidator>();
        services.AddSingleton<TravelTimeCalculator>();
        services.AddSingleton<TravelFacade>();

        // Market Subsystem
        services.AddSingleton<MarketSubsystemManager>();
        services.AddSingleton<PriceManager>();
        services.AddSingleton<ArbitrageCalculator>();
        services.AddSingleton<MarketStateTracker>();
        services.AddSingleton<MarketFacade>();

        // Exchange Subsystem
        services.AddSingleton<ExchangeValidator>();
        services.AddSingleton<ExchangeProcessor>();
        services.AddSingleton<ExchangeInventory>();
        services.AddSingleton<ExchangeOrchestrator>();
        services.AddSingleton<ExchangeFacade>();

        // Token Subsystem
        services.AddSingleton<ConnectionTokenManager>();
        services.AddSingleton<TokenEffectProcessor>();
        services.AddSingleton<TokenUnlockManager>();
        services.AddSingleton<RelationshipTracker>();
        services.AddSingleton<TokenFacade>();

        // Narrative Subsystem
        services.AddSingleton<ObservationManagerWrapper>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<NarrativeRenderer>();
        services.AddSingleton<LocationNarrativeGenerator>();
        services.AddSingleton<NarrativeFacade>();

        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();


        return services;
    }


}