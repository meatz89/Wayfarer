using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;
using Wayfarer.UIHelpers;
using Wayfarer.Content;
using Wayfarer.Services;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        string contentDirectory = "Content";

        // Create GameWorldInitializer
        GameWorldInitializer gameWorldInitializer = new GameWorldInitializer(contentDirectory);
        services.AddSingleton(gameWorldInitializer);

        // Load game state
        GameWorld gameWorld = gameWorldInitializer.LoadGame();
        services.AddSingleton(gameWorld);

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<LetterTemplateRepository>();
        services.AddSingleton<StandingObligationRepository>();

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<EncounterFactory>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<ActionProcessor>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var playerProgression = serviceProvider.GetRequiredService<PlayerProgression>();
            var environmentalPropertyManager = serviceProvider.GetRequiredService<LocationPropertyManager>();
            var locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var routeUnlockManager = serviceProvider.GetRequiredService<RouteUnlockManager>();
            var npcLetterOfferService = serviceProvider.GetRequiredService<NPCLetterOfferService>();
            return new ActionProcessor(gameWorld, playerProgression, environmentalPropertyManager, locationRepository, messageSystem, routeUnlockManager, npcLetterOfferService);
        });
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameWorldManager>();
        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<PersistentChangeProcessor>();
        services.AddSingleton<LocationPropertyManager>();

        // TimeManager is created and managed by GameWorld, not DI container
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();
        
        // Letter Queue System
        services.AddSingleton<StandingObligationManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            return new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepository);
        });
        services.AddSingleton<LetterQueueManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var obligationManager = serviceProvider.GetRequiredService<StandingObligationManager>();
            var letterQueueManager = new LetterQueueManager(gameWorld, letterTemplateRepository, npcRepository, messageSystem, obligationManager);
            
            // Set up letter chain manager after both are created
            var letterChainManager = new LetterChainManager(gameWorld, letterTemplateRepository, npcRepository, letterQueueManager, messageSystem);
            letterQueueManager.SetLetterChainManager(letterChainManager);
            
            return letterQueueManager;
        });
        services.AddSingleton<ConnectionTokenManager>();
        services.AddSingleton<RouteUnlockManager>();
        services.AddSingleton<NPCLetterOfferService>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var connectionTokenManager = serviceProvider.GetRequiredService<ConnectionTokenManager>();
            var letterQueueManager = serviceProvider.GetRequiredService<LetterQueueManager>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            return new NPCLetterOfferService(gameWorld, npcRepository, letterTemplateRepository, connectionTokenManager, letterQueueManager, messageSystem);
        });
        services.AddSingleton<LetterChainManager>(serviceProvider =>
        {
            // Get the already-created chain manager from the letter queue manager
            var letterQueueManager = serviceProvider.GetRequiredService<LetterQueueManager>();
            // The chain manager is already set up in the letter queue manager factory
            // This is a bit hacky but works around the circular dependency
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            return new LetterChainManager(gameWorld, letterTemplateRepository, npcRepository, letterQueueManager, messageSystem);
        });
        services.AddSingleton<MorningActivitiesManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var letterQueueManager = serviceProvider.GetRequiredService<LetterQueueManager>();
            var obligationManager = serviceProvider.GetRequiredService<StandingObligationManager>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            return new MorningActivitiesManager(gameWorld, letterQueueManager, obligationManager, messageSystem);
        });

        services.AddScoped<MusicService>();

        // UI Razor Services
        services.AddSingleton<CardSelectionService>();
        services.AddSingleton<CardHighlightService>();
        
        // Navigation Service
        services.AddSingleton<NavigationService>();

        services.AddAIServices();

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
                default:
                    services.AddSingleton<IAIProvider, OllamaProvider>();
                    break;
            }
        }

        return services;
    }

}