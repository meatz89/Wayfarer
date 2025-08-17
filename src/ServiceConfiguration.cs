using Wayfarer.GameState;
using Wayfarer.Services;

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
        services.AddSingleton<LetterTemplateFactory>();
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
        services.AddSingleton<LetterTemplateRepository>();

        // Register content fallback service for resilience
        services.AddSingleton<ContentFallbackService>();

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<LocationPropertyManager>();
        services.AddTimeSystem();

        // Literary UI System - attention, NPC states, verb contextualization
        services.AddLiteraryUISystem();

        // Register NPCStateResolver with all dependencies
        services.AddSingleton<NPCStateResolver>();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();
        services.AddSingleton<CollapseManager>();

        // Letter Queue System
        services.AddSingleton<StandingObligationManager>();
        services.AddSingleton<LetterCategoryService>();
        services.AddSingleton<DeliveryConversationService>();

        // Conversation System
        services.AddSingleton<DeterministicStreamingService>();
        services.AddSingleton<ConversationStateManager>();
        services.AddSingleton<NPCDeckFactory>();

        // Wire up circular dependencies after initial creation
        services.AddSingleton<TokenMechanicsManager>();


        // Leverage Calculator for power dynamics

        // Relationship tracking for contextual conversations
        services.AddSingleton<Wayfarer.GameState.NPCRelationshipTracker>();

        // Contextual conversation system
        // Removed - using existing ConversationChoiceGenerator instead

        // Card-based conversation system - no confrontation service needed

        // Environmental Storytelling Systems
        services.AddSingleton<Wayfarer.GameState.WorldMemorySystem>();
        services.AddSingleton<Wayfarer.GameState.AmbientDialogueSystem>();
        services.AddSingleton<AtmosphereCalculator>();

        services.AddSingleton<LetterQueueManager>();
        services.AddSingleton<EndorsementManager>();

        // Transaction and Preview System
        services.AddSingleton<AccessRequirementChecker>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<NetworkUnlockManager>();
        services.AddSingleton<InformationDiscoveryManager>();
        services.AddSingleton<SpecialLetterHandler>();
        services.AddSingleton<SpecialLetterGenerationService>();
        services.AddSingleton<InformationRevealService>();
        services.AddSingleton<NPCLetterOfferService>();
        services.AddSingleton<PatronLetterService>();
        services.AddSingleton<NetworkReferralService>();
        services.AddSingleton<MorningActivitiesManager>();
        services.AddSingleton<NoticeBoardService>();

        // Conversation system components
        // ConversationChoiceGenerator is now created per-conversation in ConversationFactory
        services.AddSingleton<ConversationFactory>();

        // Context and tag calculation
        services.AddSingleton<ContextTagCalculator>();

        // Attention management
        services.AddSingleton<TimeBlockAttentionManager>();

        // Action generation service
        services.AddSingleton<ActionGenerator>();

        // Core services
        services.AddSingleton<FlagService>();
        services.AddSingleton<ConversationRepository>();

        services.AddScoped<MusicService>();
        services.AddScoped<TimeImpactCalculator>();
        // ActionExecutionService removed - using intent-based architecture


        // Game Facade - THE single entry point for all UI-Backend communication
        services.AddSingleton<GameFacade>();
        services.AddSingleton<ILetterQueueOperations>(provider => provider.GetRequiredService<GameFacade>());
        services.AddScoped<NavigationCoordinator>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LetterGenerationService>();

        // UI Razor Services

        // Navigation Service - remove duplicate registration

        // State Management Services - removed GameStateManager (legacy)

        services.AddAIServices();

        return services;
    }

    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<ConversationHistoryManager>();
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<LoadingStateService>();
        // Commented out until IAIProvider is configured
        // services.AddSingleton<AIGameMaster>();
        // services.AddSingleton<AIClient>();

        // Register updated services
        services.AddSingleton<AIPromptBuilder>();
        services.AddSingleton<ConversationChoiceResponseParser>();
        services.AddSingleton<ChoiceProjectionService>();

        // Register narrative provider - choose which implementation to use
        // For POC: Using DeterministicNarrativeProvider
        services.AddSingleton<INarrativeProvider, DeterministicNarrativeProvider>();

        // For full game with AI: Uncomment this and comment out the line above
        // services.AddSingleton<INarrativeProvider, AIGameMaster>();

        //// Register AI provider factory
        //services.AddSingleton<IAIProvider>(serviceProvider =>
        //{
        //    string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "Ollama";

        //    return defaultProvider.ToLower() switch
        //    {
        //        "ollama" => new OllamaProvider(configuration, logger),
        //        _ => new OllamaProvider(configuration, logger)
        //    };
        //});

        return services;
    }

}