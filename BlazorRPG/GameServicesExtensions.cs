public static class GameServicesExpressures
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        string contentDirectory = "content";

        // Create ContentLoader
        ContentLoader contentLoader = new ContentLoader(contentDirectory);
        services.AddSingleton(contentLoader);

        // Load game state
        GameState gameState = contentLoader.LoadGame();
        services.AddSingleton(gameState);

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories
        services.AddSingleton<ActionRepository>();
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<TravelManager>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<OpportunitySystem>();
        services.AddSingleton<EncounterSystem>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<ChoiceRepository>();
        services.AddSingleton<ActionProcessor>();
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
        services.AddSingleton<EnvironmentalPropertyManager>();

        return services;
    }
}