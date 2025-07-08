using Wayfarer.UIHelpers;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        string contentDirectory = "Content";

        // Create GameWorldInitializer
        GameWorldInitializer contentLoader = new GameWorldInitializer(contentDirectory);
        services.AddSingleton(contentLoader);

        // Load game state
        GameWorld gameWorld = contentLoader.LoadGame();
        services.AddSingleton(gameWorld);

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<ContractRepository>();

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<ContractSystem>();
        services.AddSingleton<EncounterFactory>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<ActionProcessor>();
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameWorldManager>();
        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<PersistentChangeProcessor>();
        services.AddSingleton<LocationPropertyManager>();
        
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();

        services.AddScoped<MusicService>();

        // UI Razor Services
        services.AddSingleton<CardSelectionService>();
        services.AddSingleton<CardHighlightService>();

        services.AddAIServices();

        return services;
    }

    /// <summary>
    /// Configure services for economic POC - excludes AI services completely
    /// Use this for economic gameplay testing without AI dependencies
    /// </summary>
    public static IServiceCollection ConfigureEconomicServices(this IServiceCollection services)
    {
        string contentDirectory = "Content";

        // Create GameWorldInitializer
        GameWorldInitializer contentLoader = new GameWorldInitializer(contentDirectory);
        services.AddSingleton(contentLoader);

        // Load game state
        GameWorld gameWorld = contentLoader.LoadGame();
        services.AddSingleton(gameWorld);

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories (ECONOMIC ONLY)
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<ContractRepository>();

        // Core economic systems
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ActionFactory>();
        // ActionGenerator excluded for economic POC (AI-dependent)
        services.AddSingleton<ContractSystem>();
        services.AddSingleton<LocationPropertyManager>();
        services.AddSingleton<ActionProcessor>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameWorldManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var itemRepository = serviceProvider.GetRequiredService<ItemRepository>();
            var encounterFactory = serviceProvider.GetRequiredService<EncounterFactory>();
            var persistentChangeProcessor = serviceProvider.GetRequiredService<PersistentChangeProcessor>();
            var locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var actionFactory = serviceProvider.GetRequiredService<ActionFactory>();
            var actionRepository = serviceProvider.GetRequiredService<ActionRepository>();
            var locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            var travelManager = serviceProvider.GetRequiredService<TravelManager>();
            var marketManager = serviceProvider.GetRequiredService<MarketManager>();
            var tradeManager = serviceProvider.GetRequiredService<TradeManager>();
            var contractSystem = serviceProvider.GetRequiredService<ContractSystem>();
            var restManager = serviceProvider.GetRequiredService<RestManager>();
            // ActionGenerator is null for economic POC
            var playerProgression = serviceProvider.GetRequiredService<PlayerProgression>();
            var actionProcessor = serviceProvider.GetRequiredService<ActionProcessor>();
            var contentLoader = serviceProvider.GetRequiredService<GameWorldInitializer>();
            var choiceProjectionService = serviceProvider.GetRequiredService<ChoiceProjectionService>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILogger<GameWorldManager>>();
            
            return new GameWorldManager(
                gameWorld, itemRepository, encounterFactory, persistentChangeProcessor,
                locationSystem, messageSystem, actionFactory, actionRepository,
                locationRepository, travelManager, marketManager, tradeManager,
                contractSystem, restManager, null, // ActionGenerator is null for economic POC
                playerProgression, actionProcessor, contentLoader,
                choiceProjectionService, configuration, logger);
        });
        
        // Required by GameWorldManager but minimal for economic POC
        services.AddSingleton<EncounterFactory>(serviceProvider => 
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var choiceProjectionService = serviceProvider.GetRequiredService<ChoiceProjectionService>();
            var logger = serviceProvider.GetRequiredService<ILogger<EncounterFactory>>();
            
            // Use the economic constructor (no AI dependencies)
            return new EncounterFactory(gameWorld, choiceProjectionService, logger);
        });
        
        services.AddSingleton<PersistentChangeProcessor>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<ChoiceProjectionService>();
        
        // Economic managers
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();

        // UI Razor Services (minimal for economic testing)
        services.AddSingleton<CardSelectionService>();
        services.AddSingleton<CardHighlightService>();

        // DO NOT ADD AI SERVICES FOR ECONOMIC POC
        // services.AddAIServices(); // EXCLUDED FOR ECONOMIC FLOW

        return services;
    }

    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<ConversationHistoryManager>();
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<PostEncounterEvolutionParser>();
        services.AddSingleton<LoadingStateService>();
        services.AddSingleton<AIGameMaster>();
        services.AddSingleton<AIClient>();

        // Register updated services
        services.AddSingleton<AIPromptBuilder>();
        services.AddSingleton<EncounterChoiceResponseParser>();
        services.AddSingleton<ChoiceProjectionService>();

        // Get configuration to determine which provider to use
        using (ServiceProvider sp = services.BuildServiceProvider())
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "Ollama";

            // Register the appropriate AI service based on configuration
            switch (defaultProvider.ToLower())
            {
                case "ollama":
                    services.AddSingleton<IAIProvider, OllamaProvider>();
                    break;
                default: // Default to ollama
                    services.AddSingleton<IAIProvider, OllamaProvider>();
                    break;
            }
        }

        return services;
    }

}