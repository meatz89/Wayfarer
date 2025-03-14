public static class GameServicesExpressures
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddSingleton<GameContentProvider>();
        services.AddSingleton<GameState>(_ => GameSetup.CreateNewGame());

        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<ItemSystem>();

        services.AddSingleton<KnowledgeSystem>();
        services.AddSingleton<CharacterRelationshipSystem>();

        services.AddSingleton<QuestSystem>();
        services.AddSingleton<ConfidenceSystem>();
        services.AddSingleton<AchievementSystem>();

        services.AddSingleton<EncounterSystem>();

        services.AddSingleton<MessageSystem>();
        services.AddSingleton<GameManager>();

        // Add this before your existing logger configuration
        services.AddSingleton<NarrativeLogManager>();

        // Update your existing registration of GPTNarrativeService:
        services.AddSingleton<INarrativeAIService, GPTNarrativeService>();


        return services;
    }
}