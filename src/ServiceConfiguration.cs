using Microsoft.AspNetCore.Components;

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

        // GameWorld registered in Program.cs via static GameWorldInitializer.CreateGameWorld()
        // No lambda, no DI dependencies, pure static initialization

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

        // TimeManager registered in Program.cs via GameWorldInitializer.CreateInitializationResult()
        // TimeModel is a domain model with state - created explicitly, not DI-resolved

        // Managers that depend on TimeManager
        services.AddSingleton<TravelManager>();
        services.AddSingleton<TransportCompatibilityValidator>();

        // DeliveryObligation Queue System
        services.AddSingleton<StandingObligationManager>();

        // ConversationSubsystem services
        services.AddSingleton<MomentumManager>();
        services.AddSingleton<SocialEffectResolver>();
        services.AddSingleton<SocialChallengeDeckBuilder>();
        services.AddSingleton<SocialResourceCalculator>();
        services.AddSingleton<SocialFacade>();
        services.AddSingleton<MentalFacade>();
        services.AddSingleton<PhysicalFacade>();

        // Procedural Content Tracing System (debugging tool)
        services.AddSingleton<ProceduralContentTracer>();

        // Spawn Graph Builder (scoped - depends on IJSRuntime)
        services.AddScoped<SpawnGraphBuilder>();

        // Scene-Situation Architecture
        services.AddSingleton<ConsequenceFacade>();
        services.AddSingleton<SituationFacade>();
        services.AddSingleton<SceneFacade>();
        services.AddSingleton<RewardApplicationService>();

        // Unified Action Architecture - Executors (FALLBACK SCENE ARCHITECTURE)
        // LocationActionExecutor: Validates atmospheric (fallback scene) actions
        // SituationChoiceExecutor: Validates ALL ChoiceTemplate-based actions (HIGHLANDER)
        services.AddSingleton<LocationActionExecutor>();
        services.AddSingleton<SituationChoiceExecutor>();

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
        services.AddSingleton<SpawnService>();
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

        // HttpClient for Icon component (SVG loading from wwwroot)
        services.AddScoped<HttpClient>(sp =>
        {
            NavigationManager navigationManager = sp.GetRequiredService<NavigationManager>();
            return new HttpClient
            {
                BaseAddress = new Uri(navigationManager.BaseUri)
            };
        });

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
        services.AddSingleton<ScenePromptBuilder>(); // AI prompt builder for Pass 2 narrative
        services.AddSingleton<SceneNarrativeService>(); // Depends on ScenePromptBuilder, OllamaClient
        services.AddSingleton<SceneGenerationFacade>(); // MOVED: Must be before PackageLoader (dependency)
        services.AddSingleton<PackageLoader>(); // Depends on SceneGenerationFacade
        services.AddSingleton<HexRouteGenerator>();
        services.AddSingleton<SceneInstantiator>();
        services.AddSingleton<SpawnedScenePlayabilityValidator>(); // Runtime validation for soft-lock prevention

        // Dynamic Location Generation System (All locations persist within session)
        // HIGHLANDER: Location.HexPosition is source of truth - no sync service needed
        services.AddSingleton<VenueGeneratorService>(); // Procedural venue generation
        services.AddSingleton<LocationPlacementService>(); // HIGHLANDER: Single procedural hex placement for ALL locations
        services.AddSingleton<LocationPlayabilityValidator>(); // Fail-fast playability validation (all locations)

        // Scene Generation Services
        services.AddSingleton<ContentGenerationFacade>();

        // Infinite A-Story Generation (procedural main story continuation)
        services.AddSingleton<ProceduralAStoryService>(); // Depends on GameWorld, ContentGenerationFacade, PackageLoader

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
        services.AddSingleton<LocationAccessibilityService>();
        services.AddSingleton<MovementValidator>();
        services.AddSingleton<NPCLocationTracker>();
        services.AddSingleton<LocationActionManager>();
        services.AddSingleton<LocationNarrativeGenerator>();
        services.AddSingleton<LocationChallengeBuilder>();
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
        services.AddSingleton<MasteryCubeService>();
        services.AddSingleton<CubeFacade>();

        // Screen Expansion Subsystems - ConversationTree, Observation, Emergency
        services.AddSingleton<ConversationTreeFacade>();
        services.AddSingleton<ObservationFacade>();
        services.AddSingleton<EmergencyFacade>();

        // Composed Services (FACADE ISOLATION compliant - only uses services, not facades)
        // DebugCommandHandler is compliant: uses RewardApplicationService (a service), not facades
        // InteractionHistoryRecorder and TimeAdvancementOrchestrator were rolled back into
        // GameOrchestrator because they required facade access (violates "Service → never facades")
        services.AddSingleton<DebugCommandHandler>();

        // Game Orchestrator - THE single entry point for all UI-Backend communication
        // FACADE ISOLATION: Only GameOrchestrator can coordinate between facades
        services.AddSingleton<GameOrchestrator>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LoadingStateService>();

        return services;
    }

}