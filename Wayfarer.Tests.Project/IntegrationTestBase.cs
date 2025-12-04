using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.Tests;

/// <summary>
/// Base class for INTEGRATION tests providing GameWorld and service access.
/// Uses EXACT same initialization as Program.cs - tests verify production code path.
///
/// ARCHITECTURAL RULE:
/// - Integration tests: Use this base class (full production initialization)
/// - Unit tests: Create `new GameWorld()` manually, never call GameWorldInitializer
/// </summary>
public class IntegrationTestBase
{
    private GameInitializationResult _initResult;
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// Get GameWorld from production initialization.
    /// Uses CreateInitializationResult() - same as Program.cs.
    /// </summary>
    protected GameWorld GetGameWorld()
    {
        EnsureInitialized();
        return _initResult.GameWorld;
    }

    /// <summary>
    /// Get TimeManager from production initialization.
    /// Uses CreateInitializationResult() - same as Program.cs.
    /// </summary>
    protected TimeManager GetTimeManager()
    {
        EnsureInitialized();
        return _initResult.TimeManager;
    }

    /// <summary>
    /// Get or initialize GameOrchestrator for testing.
    /// Initializes full service container with all dependencies.
    /// </summary>
    protected GameOrchestrator GetGameOrchestrator()
    {
        EnsureServiceProvider();
        return _serviceProvider.GetRequiredService<GameOrchestrator>();
    }

    /// <summary>
    /// Get service from DI container.
    /// Initializes service provider if not already initialized.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        EnsureServiceProvider();
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Ensure GameWorld and TimeManager are initialized via production path.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initResult == null)
        {
            // EXACT same call as Program.cs
            _initResult = GameWorldInitializer.CreateInitializationResult();
        }
    }

    /// <summary>
    /// Initialize service provider matching Program.cs EXACTLY.
    /// GameWorld, TimeManager, and IConfiguration registered before ConfigureServices.
    /// </summary>
    private void EnsureServiceProvider()
    {
        if (_serviceProvider != null) return;

        EnsureInitialized();

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

        // EXACT same registration as Program.cs
        services.AddSingleton(_initResult.GameWorld);
        services.AddSingleton(_initResult.TimeManager);

        // Then configure all other services (same as Program.cs)
        ServiceConfiguration.ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }
}
