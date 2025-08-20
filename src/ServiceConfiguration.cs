

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        Console.WriteLine("[SERVICE] Starting service configuration...");

        // Register configuration
        Console.WriteLine("[SERVICE] Registering IContentDirectory...");
        services.AddSingleton<IContentDirectory>(_ => new ContentDirectory { Path = "Content" });

        // Register content validation
        services.AddSingleton<ValidatedContentLoader>();

        // Register game configuration and rule engine
        services.AddSingleton<GameConfigurationLoader>();
        services.AddSingleton<GameConfiguration>();
        services.AddSingleton<IGameRuleEngine, GameRuleEngine>();

        // Register factories for reference-safe content creation
        services.AddSingleton<LocationFactory>();
        services.AddSingleton<LocationSpotFactory>();
        services.AddSingleton<NPCFactory>();
        services.AddSingleton<ItemFactory>();
        services.AddSingleton<RouteFactory>();
        services.AddSingleton<RouteDiscoveryFactory>();
        services.AddSingleton<NetworkUnlockFactory>();
        services.AddSingleton<StandingObligationFactory>();

        // Register GameWorld using static GameWorldInitializer
        Console.WriteLine("[SERVICE] Registering GameWorld...");
        services.AddSingleton<GameWorld>(_ =>
        {
            // Call GameWorldInitializer statically - no DI dependencies needed
            return GameWorldInitializer.CreateGameWorld();
        });
        Console.WriteLine("[SERVICE] GameWorld registered");

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Core services that have no dependencies
        services.AddSingleton<NPCVisibilityService>();

        // Register repositories
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<LocationSpotRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<StandingObligationRepository>();
        services.AddSingleton<RouteDiscoveryRepository>();
        services.AddSingleton<NetworkUnlockRepository>();

        // Register content fallback service for resilience
        services.AddSingleton<ContentFallbackService>();

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();
        // services.AddSingleton<WorldStateInputBuilder>(); // Removed - no longer exists
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        // services.AddSingleton<LocationCreationSystem>(); // Removed - no longer exists
        services.AddSingleton<LocationPropertyManager>();
        services.AddTimeSystem();

        // Literary UI System - attention, NPC states, verb contextualization
        services.AddLiteraryUISystem();

        // Register NPCStateResolver with all dependencies
        services.AddSingleton<NPCStateResolver>();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelEventManager>();
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();
        // services.AddSingleton<CollapseManager>(); // Removed - no longer exists

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();
        // services.AddSingleton<ConversationContextService>(); // Removed - no longer exists

        // New card-based conversation system
        services.AddSingleton<ConversationManager>();
        services.AddSingleton<NPCDeckFactory>();

        // Wire up circular dependencies after initial creation
        services.AddSingleton<TokenMechanicsManager>();
        services.AddSingleton<EndingGenerator>();


        // Leverage Calculator for power dynamics

        // Relationship tracking for contextual conversations
        services.AddSingleton<NPCRelationshipTracker>();

        // Contextual conversation system
        // Removed - using existing ConversationChoiceGenerator instead

        // Card-based conversation system - no confrontation service needed

        // Environmental Storytelling Systems
        services.AddSingleton<WorldMemorySystem>();
        services.AddSingleton<AmbientDialogueSystem>();
        services.AddSingleton<AtmosphereCalculator>();

        services.AddSingleton<ObligationQueueManager>();

        // Transaction and Preview System
        services.AddSingleton<AccessRequirementChecker>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<NetworkUnlockManager>();
        services.AddSingleton<InformationDiscoveryManager>();
        services.AddSingleton<SpecialLetterHandler>();
        // Letter generation now handled through conversation cards
        // PatronLetterService removed - patron system deleted
        services.AddSingleton<NetworkReferralService>();
        services.AddSingleton<DailyActivitiesManager>();

        // Old conversation system removed

        // Context and tag calculation
        services.AddSingleton<ContextTagCalculator>();

        // Attention management
        services.AddSingleton<TimeBlockAttentionManager>();

        // Action generation service
        services.AddSingleton<ActionGenerator>();

        // Core services
        services.AddSingleton<FlagService>();
        // ConversationRepository removed - using new card system

        services.AddScoped<MusicService>();
        services.AddScoped<TimeImpactCalculator>();
        // ActionExecutionService removed - using intent-based architecture


        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddScoped<NavigationCoordinator>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();

        // UI Razor Services

        // Navigation Service - remove duplicate registration

        // State Management Services - removed GameStateManager (legacy)

        // AI services removed - using deterministic card system

        return services;
    }


}