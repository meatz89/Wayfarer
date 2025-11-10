
public static class ServiceConfiguration
{
public static IServiceCollection ConfigureServices(this IServiceCollection services)
{
    // Register dev mode service (needs to be early for other services to use)
    services.AddSingleton<DevModeService>();

    // Register configuration
    services.AddSingleton<IContentDirectory, ContentDirectory>();

    // Register game configuration and rule engine
    services.AddSingleton<GameConfiguration>();
    services.AddSingleton<IGameRuleEngine, GameRuleEngine>();

    // Register GameWorld using static GameWorldInitializer
    services.AddSingleton<GameWorld>(_ =>
    {
        // Call GameWorldInitializer statically - no DI dependencies needed
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
        return gameWorld;
    });

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

    // V3 Card-Based Obligation System - DELETED (wrong architecture)
    // Obligation is strategic activity, not tactical system
    // Mental/Physical facades will be added in refactor

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
    services.AddSingleton<SocialFacade>();
    services.AddSingleton<MentalFacade>();
    services.AddSingleton<PhysicalFacade>();

    // Scene-Situation Architecture
    services.AddSingleton<ConsequenceFacade>();
    services.AddSingleton<SituationFacade>();
    services.AddSingleton<SceneFacade>();
    services.AddSingleton<SpawnFacade>();
    services.AddSingleton<RewardApplicationService>();

    // Unified Action Architecture - Three Parallel Executors
    services.AddSingleton<LocationActionExecutor>();
    services.AddSingleton<NPCActionExecutor>();
    services.AddSingleton<PathCardExecutor>();

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

    // Obligation Activity - Strategic orchestrator for multi-phase obligations
    services.AddSingleton<ObligationActivity>();
    services.AddSingleton<ObligationDiscoveryEvaluator>();

    // Scene and Situation Services - Situation visibility filtering with property + access requirements
    services.AddSingleton<SituationCompletionHandler>();
    services.AddSingleton<DifficultyCalculationService>();
    // SceneFacade removed - old architecture deleted

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

    // Environmental Storytelling Systems
    // ObservationManager eliminated - observation system removed
    services.AddSingleton<ObservationSystem>();
    services.AddSingleton<BindingObligationSystem>();

    // Scene Instantiation System (needed by ObligationActivity)
    services.AddSingleton<SpawnConditionsEvaluator>();
    services.AddSingleton<SceneNarrativeService>();
    services.AddSingleton<PackageLoader>();
    services.AddSingleton<HexRouteGenerator>();
    services.AddSingleton<MarkerResolutionService>();
    services.AddSingleton<SceneInstantiator>();
    services.AddSingleton<DependentResourceOrchestrationService>();

    // Scene Generation and Instance Facades (clean boundaries for procedural content)
    // IMPORTANT: Register dependencies BEFORE SceneInstanceFacade
    services.AddSingleton<PackageLoaderFacade>();
    services.AddSingleton<ContentGenerationFacade>();
    services.AddSingleton<SceneGenerationFacade>();
    services.AddSingleton<SceneInstanceFacade>(); // Depends on PackageLoaderFacade + ContentGenerationFacade

    // Infinite A-Story Generation (procedural main story continuation)
    services.AddSingleton<ProceduralAStoryService>(); // Depends on GameWorld, ContentGenerationFacade, PackageLoaderFacade

    // State Clearing System (needed by TimeFacade)
    services.AddSingleton<StateClearingResolver>();

    // Transaction and Preview System
    // AccessRequirementChecker eliminated - PRINCIPLE 4: Economic affordability determines access
    services.AddSingleton<NarrativeService>();

    // Action generation service DELETED - ActionGenerator.cs removed (violates three-tier timing)
    // Actions now created by SceneFacade at query time from ChoiceTemplates

    // Core services
    services.AddScoped<MusicService>();

    // Venue Subsystem
    services.AddSingleton<LocationManager>();
    services.AddSingleton<MovementValidator>();
    services.AddSingleton<NPCLocationTracker>();
    services.AddSingleton<LocationActionManager>();
    services.AddSingleton<LocationNarrativeGenerator>();
    services.AddSingleton<LocationFacade>();

    // Obligation Subsystem
    services.AddSingleton<MeetingManager>();

    // Time Subsystem (registered BEFORE ResourceFacade - dependency ordering)
    services.AddSingleton<TimeBlockCalculator>();
    services.AddSingleton<TimeProgressionManager>();
    services.AddSingleton<TimeDisplayFormatter>();
    services.AddSingleton<TimeFacade>();

    // Resource Subsystem (depends on TimeFacade)
    services.AddSingleton<ResourceCalculator>();
    services.AddSingleton<ResourceFacade>();

    // Travel Subsystem
    services.AddSingleton<RouteManager>();
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
    services.AddSingleton<ExchangeOrchestrator>();
    services.AddSingleton<ExchangeFacade>();

    // Token Subsystem
    services.AddSingleton<ConnectionTokenManager>();
    services.AddSingleton<TokenEffectProcessor>();
    services.AddSingleton<TokenUnlockManager>();
    services.AddSingleton<RelationshipTracker>();
    services.AddSingleton<TokenFacade>();

    // Narrative Subsystem
    services.AddSingleton<MessageSystem>();
    services.AddSingleton<NarrativeService>();
    services.AddSingleton<NarrativeRenderer>();
    services.AddSingleton<LocationNarrativeGenerator>();
    services.AddSingleton<NarrativeFacade>();

    // Mastery (Cubes) Subsystem
    services.AddSingleton<CubeFacade>();

    // Screen Expansion Subsystems - ConversationTree, Observation, Emergency
    services.AddSingleton<ConversationTreeFacade>();
    services.AddSingleton<ObservationFacade>();
    services.AddSingleton<EmergencyFacade>();

    // Game Facade - THE single entry point for all UI-Backend communication
    services.AddSingleton<GameFacade>();
    services.AddSingleton<NPCService>();
    services.AddSingleton<LoadingStateService>();

    return services;
}

}