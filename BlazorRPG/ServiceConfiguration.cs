using BlazorRPG.UIHelpers;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        string contentDirectory = "content";

        // Create ContentLoader
        ContentLoader contentLoader = new ContentLoader(contentDirectory);
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