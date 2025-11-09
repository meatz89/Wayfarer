using Microsoft.Extensions.DependencyInjection;
using Wayfarer.GameState;
using Wayfarer.Services;

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
    /// Get or initialize GameFacade for testing.
    /// Initializes full service container with all dependencies.
    /// </summary>
    protected GameFacade GetGameFacade()
    {
        if (_serviceProvider == null)
        {
            ServiceCollection services = new ServiceCollection();
            ServiceConfiguration.ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        return _serviceProvider.GetRequiredService<GameFacade>();
    }

    /// <summary>
    /// Get service from DI container.
    /// Initializes service provider if not already initialized.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            ServiceCollection services = new ServiceCollection();
            ServiceConfiguration.ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        return _serviceProvider.GetRequiredService<T>();
    }
}
