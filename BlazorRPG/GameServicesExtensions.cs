public static class GameServicesExpressures
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddSingleton<GameContentProvider>();
        services.AddSingleton<GameState>(_ => GameSetup.CreateNewGame());

        services.AddSingleton<ActionFactory>();
        services.AddSingleton<ActionGenerator>();
        services.AddSingleton<ActionRepository>();

        services.AddSingleton<TravelManager>();
        services.AddSingleton<ItemSystem>();

        services.AddSingleton<KnowledgeSystem>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<OpportunitySystem>();
        services.AddSingleton<EncounterSystem>();
        services.AddSingleton<ActionSystem>();

        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameManager>();

        // Add this before your existing logger configuration
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<NarrativeContextManager>();

        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<PostEncounterEvolutionSystem>();

        services.AddSingleton<ResourceManager>();
        services.AddSingleton<NarrativeService>();

        services.AddSingleton<PostEncounterEvolutionParser>();
        services.AddSingleton<TutorialState>();

        return services;
    }
}