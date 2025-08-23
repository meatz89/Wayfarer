using System.IO;

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
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        services.AddTimeSystem();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelEventManager>();
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        // services.AddSingleton<RestManager>(); // Commented out - class doesn't exist
        services.AddSingleton<TransportCompatibilityValidator>();

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();

        // New card-based conversation system
        services.AddSingleton<ConversationManager>();
        services.AddSingleton<NPCDeckFactory>();
        
        // Dialogue generation services (NO hardcoded text)
        services.AddSingleton<DialogueGenerationService>(provider => 
            new DialogueGenerationService(Path.Combine("Content", "Templates")));
        services.AddSingleton<NarrativeRenderer>();

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
        services.AddSingleton<NetworkUnlockManager>();
        services.AddSingleton<SpecialLetterHandler>();
        services.AddSingleton<NetworkReferralService>();
        services.AddSingleton<DailyActivitiesManager>();

        // Attention management
        services.AddSingleton<TimeBlockAttentionManager>();

        // Action generation service
        services.AddSingleton<ActionGenerator>();

        // Core services
        services.AddSingleton<FlagService>();

        services.AddScoped<MusicService>();
        services.AddScoped<TimeImpactCalculator>();

        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddScoped<NavigationCoordinator>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();

        return services;
    }


}