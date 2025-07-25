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
        services.AddSingleton<GameConfiguration>(serviceProvider =>
        {
            GameConfigurationLoader loader = serviceProvider.GetRequiredService<GameConfigurationLoader>();
            return loader.LoadConfiguration();
        });
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

        // Register GameWorldInitializer as both itself and IGameWorldFactory
        Console.WriteLine("[SERVICE] Registering GameWorldInitializer...");
        services.AddSingleton<GameWorldInitializer>();
        services.AddSingleton<IGameWorldFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<GameWorldInitializer>());
        Console.WriteLine("[SERVICE] GameWorldInitializer registered");

        // Register GameWorld using factory pattern
        Console.WriteLine("[SERVICE] Registering GameWorld factory...");
        services.AddSingleton<GameWorld>(serviceProvider =>
        {
            Console.WriteLine("[SERVICE] Creating GameWorld via factory...");
            ILogger<GameWorld> logger = serviceProvider.GetRequiredService<ILogger<GameWorld>>();
            logger.LogInformation("GameWorld factory method called");

            IGameWorldFactory factory = serviceProvider.GetRequiredService<IGameWorldFactory>();
            logger.LogInformation("Got IGameWorldFactory instance");

            GameWorld gameWorld = factory.CreateGameWorld();
            logger.LogInformation("GameWorld instance created successfully");

            Console.WriteLine("[SERVICE] GameWorld created via factory");
            return gameWorld;
        });
        Console.WriteLine("[SERVICE] GameWorld factory registered");

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<LocationSpotRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<StandingObligationRepository>();
        services.AddSingleton<RouteDiscoveryRepository>();
        services.AddSingleton<NetworkUnlockRepository>();
        services.AddSingleton<LetterTemplateRepository>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var letterTemplateRepository = new LetterTemplateRepository(gameWorld);
            
            // Wire up after construction
            var categoryService = serviceProvider.GetRequiredService<LetterCategoryService>();
            letterTemplateRepository.SetCategoryService(categoryService);
            
            return letterTemplateRepository;
        });

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<DebugLogger>();

        services.AddSingleton<GameWorldManager>();
        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<LocationPropertyManager>();
        services.AddTimeSystem();

        // Managers that depend on TimeManager
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();

        // Letter Queue System
        services.AddSingleton<StandingObligationManager>();
        services.AddSingleton<LetterCategoryService>();
        services.AddSingleton<DeliveryConversationService>();

        // Conversation System
        services.AddSingleton<DeterministicStreamingService>();
        services.AddSingleton<ConversationStateManager>();

        // Wire up circular dependencies after initial creation
        services.AddSingleton<ConnectionTokenManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            
            var connectionTokenManager = new ConnectionTokenManager(gameWorld, messageSystem, npcRepository);
            
            // Defer wiring to avoid circular dependency during construction
            var categoryService = serviceProvider.GetRequiredService<LetterCategoryService>();
            var obligationManager = serviceProvider.GetRequiredService<StandingObligationManager>();
            
            connectionTokenManager.SetCategoryService(categoryService);
            connectionTokenManager.SetObligationManager(obligationManager);
            
            return connectionTokenManager;
        });

        services.AddSingleton<LetterQueueManager>();

        // Transaction and Preview System
        services.AddSingleton<INavigationHandler, NavigationHandler>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<AccessRequirementChecker>();
        services.AddSingleton<TokenFavorRepository>();
        services.AddSingleton<TokenFavorManager>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<NetworkUnlockManager>();
        services.AddSingleton<NPCLetterOfferService>();
        services.AddSingleton<PatronLetterService>();
        services.AddSingleton<NetworkReferralService>();
        services.AddSingleton<MorningActivitiesManager>();
        services.AddSingleton<NoticeBoardService>();

        // Command Discovery Service replaces LocationActionManager
        services.AddSingleton<CommandDiscoveryService>();
        services.AddSingleton<ConversationFactory>();

        // Narrative system components
        services.AddSingleton<FlagService>();
        services.AddSingleton<NarrativeManager>();
        services.AddSingleton<NarrativeRequirement>();
        services.AddSingleton<NarrativeJournal>();
        services.AddSingleton<NarrativeEffectRegistry>();

        services.AddScoped<MusicService>();

        // Command System
        services.AddSingleton<CommandExecutor>();

        // UI Services - Clean separation between UI and game logic
        services.AddSingleton<MarketUIService>();
        services.AddSingleton<LetterQueueUIService>();
        services.AddSingleton<TravelUIService>();
        services.AddSingleton<RestUIService>();
        services.AddSingleton<LocationActionsUIService>();
        services.AddSingleton<NPCService>();
        services.AddSingleton<LetterGenerationService>();

        // UI Razor Services

        // Navigation Service - remove duplicate registration

        // State Management Services
        services.AddSingleton<GameStateManager>();

        services.AddAIServices();

        return services;
    }

    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<ConversationHistoryManager>();
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<LoadingStateService>();
        services.AddSingleton<AIGameMaster>();
        services.AddSingleton<AIClient>();

        // Register updated services
        services.AddSingleton<AIPromptBuilder>();
        services.AddSingleton<ConversationChoiceResponseParser>();
        services.AddSingleton<ChoiceProjectionService>();

        // Register narrative provider - choose which implementation to use
        // For POC: Using DeterministicNarrativeProvider
        services.AddSingleton<INarrativeProvider, DeterministicNarrativeProvider>();

        // For full game with AI: Uncomment this and comment out the line above
        // services.AddSingleton<INarrativeProvider, AIGameMaster>();

        // Register AI provider factory
        services.AddSingleton<IAIProvider>(serviceProvider =>
        {
            IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
            ILogger<ConversationFactory> logger = serviceProvider.GetRequiredService<ILogger<ConversationFactory>>();
            string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "Ollama";

            return defaultProvider.ToLower() switch
            {
                "ollama" => new OllamaProvider(configuration, logger),
                _ => new OllamaProvider(configuration, logger)
            };
        });

        return services;
    }

}