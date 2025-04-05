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
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<ItemSystem>();

        services.AddSingleton<KnowledgeSystem>();
        services.AddSingleton<CharacterRelationshipSystem>();

        services.AddSingleton<EncounterSystem>();

        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameManager>();

        // Add this before your existing logger configuration
        services.AddSingleton<NarrativeLogManager>();
        services.AddSingleton<NarrativeContextManager>();

        services.AddSingleton<LocationCreationService>();
        services.AddSingleton<PostEncounterEvolutionService>();

        services.AddSingleton<ResourceManager>();
        services.AddSingleton<NarrativeService>();

        services.AddSingleton<PostEncounterEvolutionParser>();

        return services;
    }
}