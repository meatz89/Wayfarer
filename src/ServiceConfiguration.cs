using System.IO;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        Console.WriteLine("[SERVICE] Starting service configuration...");

        // Register dev mode service (needs to be early for other services to use)
        services.AddSingleton<DevModeService>();
        
        // Register configuration
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
        services.AddSingleton<ItemFactory>();
        services.AddSingleton<RouteFactory>();
        services.AddSingleton<RouteDiscoveryFactory>();
        services.AddSingleton<StandingObligationFactory>();

        // Register GameWorld using static GameWorldInitializer
        Console.WriteLine("[SERVICE] Registering GameWorld...");
        services.AddSingleton<GameWorld>(_ =>
        {
            Console.WriteLine("[SERVICE] Creating GameWorld instance...");
            // Call GameWorldInitializer statically - no DI dependencies needed
            var gameWorld = GameWorldInitializer.CreateGameWorld();
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

        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        services.AddTimeSystem();

        // Managers that depend on TimeManager
        // TravelEventManager removed - use categorical mechanics instead
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        // services.AddSingleton<RestManager>(); // Commented out - class doesn't exist
        services.AddSingleton<TransportCompatibilityValidator>();

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();

        // New card-based conversation system - removed with ConversationSystem
        // services.AddSingleton<ConversationManager>();
        // services.AddSingleton<NPCDeckFactory>();
        
        // ConversationSubsystem services
        services.AddSingleton<CardDeckManager>();
        services.AddSingleton<EmotionalStateManager>();
        services.AddSingleton<ComfortManager>();
        services.AddSingleton<DialogueGenerator>();
        services.AddSingleton<ExchangeHandler>();
        services.AddSingleton<ConversationStateTracker>();
        services.AddSingleton<ConversationOrchestrator>();
        services.AddSingleton<ConversationFacade>();
        
        // Dialogue generation services (NO hardcoded text)
        services.AddSingleton<DialogueGenerationService>(provider => 
            new DialogueGenerationService(Path.Combine("Content", "Templates")));
        services.AddSingleton<NarrativeRenderer>();

        // Wire up circular dependencies after initial creation
        services.AddSingleton<TokenMechanicsManager>();

        // Observation management system
        services.AddSingleton<ObservationManager>();

        // Environmental Storytelling Systems
        Console.WriteLine("[SERVICE] Registering ObservationSystem...");
        services.AddSingleton<ObservationSystem>();
        Console.WriteLine("[SERVICE] ObservationSystem registered");
        services.AddSingleton<BindingObligationSystem>();

        services.AddSingleton<ObligationQueueManager>();

        // Transaction and Preview System
        services.AddSingleton<AccessRequirementChecker>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<SpecialLetterHandler>();
        services.AddSingleton<DailyActivitiesManager>();

        // Attention management
        services.AddSingleton<TimeBlockAttentionManager>();

        // Action generation service
        services.AddSingleton<ActionGenerator>();

        // Core services
        services.AddSingleton<FlagService>();

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
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.AttentionManager>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.ResourceCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.ResourceSubsystem.ResourceFacade>();

        // Time Subsystem
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeBlockCalculator>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeProgressionManager>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeDisplayFormatter>();
        services.AddSingleton<Wayfarer.Subsystems.TimeSubsystem.TimeFacade>();

        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();

        return services;
    }


}