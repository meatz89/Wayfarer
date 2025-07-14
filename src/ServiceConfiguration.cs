using Wayfarer.Game.MainSystem;
using Wayfarer.UIHelpers;

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
        services.AddSingleton<ContractRepository>();
        services.AddSingleton<RouteRepository>();

        // Register contract services
        services.AddSingleton<ContractValidationService>();
        services.AddSingleton<ContractProgressionService>();

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

        // TimeManager is created and managed by GameWorld, not DI container
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();

        services.AddScoped<MusicService>();

        // UI Razor Services
        services.AddSingleton<CardSelectionService>();
        services.AddSingleton<CardHighlightService>();

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