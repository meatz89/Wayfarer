public static class GameServicesExpressures
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        // Initialize contentRegistry and register as singleton
        ContentRegistry contentRegistry = ContentBootstrapper.InitializeRegistry();
        services.AddSingleton(contentRegistry);

        // Repositories depend on the contentRegistry
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();

        GameState game = GameSetup.CreateNewGame();
        services.AddSingleton(game);

        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<TravelManager>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<OpportunitySystem>();
        services.AddSingleton<EncounterSystem>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<CardRepository>();
        services.AddSingleton<OutcomeProcessor>();
        services.AddSingleton<EncounterFactory>();
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameManager>();
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<NarrativeContextManager>();
        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<PostEncounterEvolutionSystem>();
        services.AddSingleton<ResourceManager>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<PostEncounterEvolutionParser>();

        return services;
    }
}