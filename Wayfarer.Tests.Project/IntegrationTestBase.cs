using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.Tests;

/// <summary>
/// Base class for integration tests providing GameWorld and service access.
/// Uses real facades with test GameWorld for clean integration testing.
/// </summary>
public class IntegrationTestBase
{
    private GameWorld _gameWorld;
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// Get or initialize GameWorld for testing.
    /// Uses Content/Core for full game content (not test subset).
    /// </summary>
    protected GameWorld GetGameWorld()
    {
        if (_gameWorld == null)
        {
            _gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        }
        return _gameWorld;
    }

    /// <summary>
    /// Get or initialize GameOrchestrator for testing.
    /// Initializes full service container with all dependencies.
    /// </summary>
    protected GameOrchestrator GetGameOrchestrator()
    {
        if (_serviceProvider == null)
        {
            InitializeServiceProvider();
        }

        return _serviceProvider.GetRequiredService<GameOrchestrator>();
    }

    /// <summary>
    /// Get service from DI container.
    /// Initializes service provider if not already initialized.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            InitializeServiceProvider();
        }

        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Initialize service provider with GameWorld registered (matches Program.cs pattern).
    /// GameWorld and IConfiguration MUST be registered before ConfigureServices is called.
    /// </summary>
    private void InitializeServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();

        // Register IConfiguration (required by some services like AINarrativeProvider)
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OllamaConfiguration:BaseUrl"] = "http://localhost:11434",
                ["OllamaConfiguration:Model"] = "llama2",
                ["OllamaConfiguration:Enabled"] = "false"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Register GameWorld (same pattern as Program.cs)
        GameWorld gameWorld = GetGameWorld();
        services.AddSingleton(gameWorld);

        // Then configure all other services
        ServiceConfiguration.ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }
}
