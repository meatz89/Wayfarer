using BlazorRPG.Game.EncounterManager;

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
        services.AddSingleton<ReputationSystem>();
        services.AddSingleton<AchievementSystem>();

        services.AddSingleton<JournalSystem>();
        services.AddSingleton<NarrativeSystem>();
        services.AddSingleton<EncounterSystem>();

        services.AddSingleton<ActionValidator>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<ActionSystem>();
        services.AddSingleton<GameManager>();

        return services;
    }
}