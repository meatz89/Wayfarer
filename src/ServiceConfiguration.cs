
public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register configuration
        services.AddSingleton<IContentDirectory>(_ => new ContentDirectory { Path = "Content" });
        
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
        services.AddSingleton<ActionDefinitionFactory>();
        
        // Register GameWorldInitializer as both itself and IGameWorldFactory
        services.AddSingleton<GameWorldInitializer>();
        services.AddSingleton<IGameWorldFactory>(serviceProvider => 
            serviceProvider.GetRequiredService<GameWorldInitializer>());
        
        // Register GameWorld using factory pattern
        services.AddSingleton<GameWorld>(serviceProvider => 
        {
            var factory = serviceProvider.GetRequiredService<IGameWorldFactory>();
            return factory.CreateGameWorld();
        });

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<LocationSpotRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<LetterTemplateRepository>();
        services.AddSingleton<StandingObligationRepository>();
        services.AddSingleton<RouteDiscoveryRepository>();
        services.AddSingleton<NetworkUnlockRepository>();

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<CharacterSystem>();
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

        // TimeManager is created and managed by GameWorld, not DI container
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();
        
        // Letter Queue System
        services.AddSingleton<ConnectionTokenManager>();
        services.AddSingleton<StandingObligationManager>();
        services.AddSingleton<LetterQueueManager>();
        services.AddSingleton<RouteUnlockManager>();
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
        services.AddSingleton<LetterCarryingManager>();

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

        // Register AI provider factory
        services.AddSingleton<IAIProvider>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILogger<EncounterFactory>>();
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