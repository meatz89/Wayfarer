public static class GameServicesExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddSingleton<GameContentProvider>();
        services.AddSingleton<GameState>(_ => GameSetup.CreateNewGame());

        services.AddSingleton<NarrativeSystem>();
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();

        services.AddSingleton<KnowledgeSystem>();
        services.AddSingleton<CharacterRelationshipSystem>();
        services.AddSingleton<TimeSystem>();
        services.AddSingleton<InformationSystem>();
        services.AddSingleton<LocationAccess>();
        services.AddSingleton<ActionValidator>();

        services.AddSingleton<ActionManager>();

        return services;
    }
}