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
        services.AddSingleton<InformationSystem>();

        services.AddSingleton<QuestSystem>();
        services.AddSingleton<ReputationSystem>();
        services.AddSingleton<StatusSystem>();
        services.AddSingleton<AchievementSystem>();

        services.AddSingleton<EncounterSystem>();
        services.AddSingleton<ChoiceSystem>();

        services.AddSingleton<ActionValidator>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<GameManager>();

        return services;
    }
}