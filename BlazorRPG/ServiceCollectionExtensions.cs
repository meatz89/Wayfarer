public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddSingleton<GameContentProvider>();
        services.AddSingleton<GameState>(_ => GameSetup.CreateNewGame());
        services.AddSingleton<NarrativeSystem>();
        services.AddSingleton<ActionManager>();

        return services;
    }
}