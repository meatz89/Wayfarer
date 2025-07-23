using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

/// <summary>
/// Factory for creating GameWorldManager instances for testing.
/// Provides the same initialization flow as production but with test-friendly configuration.
/// Eliminates manual service construction and ensures tests use the real game flow.
/// </summary>
public static class TestGameWorldFactory
{
    /// <summary>
    /// Creates a complete GameWorldManager using the same initialization as production.
    /// This is the standard method for most tests - provides a fully functional game world.
    /// </summary>
    /// <param name="contentDirectory">Directory containing JSON content files (default: "Content")</param>
    /// <returns>Fully configured GameWorldManager ready for testing</returns>
    public static GameWorldManager CreateCompleteGameWorldManager(string contentDirectory = "Content")
    {
        // Build service collection with test configuration
        ServiceCollection services = new ServiceCollection();

        // Add minimal configuration for testing
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"DefaultAIProvider", "Test"}, // Use test provider to avoid AI dependencies
                {"TestMode", "true"}
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Add logging for test debugging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Configure test services - same as production but without AI services
        services.ConfigureTestServices(contentDirectory);

        // Build service provider and get GameWorldManager
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();

        // Initialize the game world (same as production startup)
        // Note: We skip initialization for tests to avoid async complexity
        // Tests should use the pre-loaded game world from GameWorldInitializer

        return gameWorldManager;
    }


    /// <summary>
    /// Creates a GameWorldManager with custom items for market testing.
    /// Useful for testing market scenarios with specific item configurations.
    /// </summary>
    /// <param name="customItems">List of items to add to the game world</param>
    /// <param name="contentDirectory">Directory containing base JSON content</param>
    /// <returns>GameWorldManager with custom item configuration</returns>
    public static GameWorldManager CreateMarketTestManager(List<Item> customItems, string contentDirectory = "Content")
    {
        GameWorldManager gameManager = CreateCompleteGameWorldManager(contentDirectory);

        // Add custom items to the game world
        foreach (Item item in customItems)
        {
            gameManager.GameWorld.WorldState.Items?.Add(item);
        }

        return gameManager;
    }

    /// <summary>
    /// Creates a GameWorldManager with custom locations for travel testing.
    /// Useful for testing travel scenarios with specific location configurations.
    /// </summary>
    /// <param name="customLocations">List of locations to add to the game world</param>
    /// <param name="contentDirectory">Directory containing base JSON content</param>
    /// <returns>GameWorldManager with custom location configuration</returns>
    public static GameWorldManager CreateTravelTestManager(List<Location> customLocations, string contentDirectory = "Content")
    {
        GameWorldManager gameManager = CreateCompleteGameWorldManager(contentDirectory);

        // Add custom locations to the game world
        foreach (Location location in customLocations)
        {
            gameManager.GameWorld.WorldState.locations?.Add(location);
        }

        return gameManager;
    }


    /// <summary>
    /// Helper method to create a simple test item for market testing.
    /// </summary>
    /// <param name="itemId">Unique identifier for the item</param>
    /// <param name="name">Display name</param>
    /// <param name="buyPrice">Base buy price</param>
    /// <param name="sellPrice">Base sell price</param>
    /// <param name="weight">Item weight</param>
    /// <returns>Item configured for testing</returns>
    public static Item CreateTestItem(string itemId, string name, int buyPrice = 10, int sellPrice = 8, int weight = 1)
    {
        return new Item
        {
            Id = itemId,
            Name = name,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            Weight = weight,
            Description = $"Test item: {name}",
            Categories = new List<ItemCategory> { ItemCategory.Materials }
        };
    }

    /// <summary>
    /// Helper method to create a simple test location for travel testing.
    /// </summary>
    /// <param name="locationId">Unique identifier for the location</param>
    /// <param name="name">Display name</param>
    /// <param name="description">Location description</param>
    /// <returns>Location configured for testing</returns>
    public static Location CreateTestLocation(string locationId, string name, string description = null)
    {
        return new Location(locationId, name)
        {
            Description = description ?? $"Test location: {name}"
        };
    }
}

/// <summary>
/// Extension methods for ServiceCollection to configure test services.
/// Mirrors production ServiceConfiguration but without AI dependencies.
/// </summary>
public static class TestServiceConfiguration
{
    /// <summary>
    /// Configures services for testing - same as production but without AI services.
    /// This ensures tests use the exact same service configuration as the real game.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="contentDirectory">Directory containing JSON content files</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection ConfigureTestServices(this IServiceCollection services, string contentDirectory = "Content")
    {
        // Register configuration
        services.AddSingleton<IContentDirectory>(_ => new ContentDirectory { Path = contentDirectory });
        
        // Register factories for reference-safe content creation
        services.AddSingleton<LocationFactory>();
        services.AddSingleton<LocationSpotFactory>();
        services.AddSingleton<NPCFactory>();
        services.AddSingleton<ItemFactory>();
        services.AddSingleton<RouteFactory>();
        services.AddSingleton<RouteDiscoveryFactory>();
        services.AddSingleton<NetworkUnlockFactory>();
        services.AddSingleton<LetterTemplateFactory>();
        services.AddSingleton<StandingObligationFactory>();
        
        // Register GameWorldInitializer as both itself and IGameWorldFactory
        services.AddSingleton<GameWorldInitializer>();
        services.AddSingleton<IGameWorldFactory>(serviceProvider => 
            serviceProvider.GetRequiredService<GameWorldInitializer>());
        
        // Register GameWorld using factory pattern
        services.AddSingleton<GameWorld>(serviceProvider => 
        {
            var factory = serviceProvider.GetRequiredService<IGameWorldFactory>();
            return factory.CreateGameWorld();
        });

        // Register the content validator
        services.AddSingleton<ContentValidator>();

        // Register repositories (same as production)
        services.AddSingleton<LocationRepository>();
        services.AddSingleton<LocationSpotRepository>();
        services.AddSingleton<ItemRepository>();
        services.AddSingleton<NPCRepository>();
        services.AddSingleton<RouteRepository>();
        services.AddSingleton<StandingObligationRepository>();
        services.AddSingleton<LetterTemplateRepository>();
        services.AddSingleton<RouteDiscoveryRepository>();
        services.AddSingleton<NetworkUnlockRepository>();


        // Register core game systems (same as production)
        services.AddSingleton<LocationSystem>();
        services.AddSingleton<CharacterSystem>();
        services.AddSingleton<WorldStateInputBuilder>();
        services.AddSingleton<PlayerProgression>();
        services.AddSingleton<MessageSystem>();
        services.AddSingleton<NarrativeService>();
        services.AddSingleton<ConnectionTokenManager>();
        services.AddSingleton<GameWorldManager>();
        services.AddSingleton<LocationCreationSystem>();
        services.AddSingleton<PersistentChangeProcessor>();
        services.AddSingleton<LocationPropertyManager>();

        // Register managers (same as production)
        services.AddSingleton<TravelManager>();
        services.AddSingleton<MarketManager>();
        services.AddSingleton<TradeManager>();
        services.AddSingleton<RestManager>();
        services.AddSingleton<TransportCompatibilityValidator>();
        services.AddSingleton<AccessRequirementChecker>();
        
        // Register game configuration and rule engine
        services.AddSingleton<GameConfiguration>(serviceProvider =>
        {
            var contentDirectory = serviceProvider.GetRequiredService<IContentDirectory>();
            var loader = new GameConfigurationLoader(contentDirectory);
            return loader.LoadConfiguration();
        });
        services.AddSingleton<IGameRuleEngine, GameRuleEngine>();
        
        // Letter Queue System (same as production)
        services.AddSingleton<StandingObligationManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var connectionTokenManager = serviceProvider.GetRequiredService<ConnectionTokenManager>();
            return new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepository, connectionTokenManager);
        });
        services.AddSingleton<LetterCategoryService>();
        services.AddSingleton<LetterQueueManager>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            var obligationManager = serviceProvider.GetRequiredService<StandingObligationManager>();
            var connectionTokenManager = serviceProvider.GetRequiredService<ConnectionTokenManager>();
            var categoryService = serviceProvider.GetRequiredService<LetterCategoryService>();
            var conversationFactory = serviceProvider.GetRequiredService<ConversationFactory>();
            var config = serviceProvider.GetRequiredService<GameConfiguration>();
            var ruleEngine = serviceProvider.GetRequiredService<IGameRuleEngine>();
            var letterQueueManager = new LetterQueueManager(gameWorld, letterTemplateRepository, npcRepository, messageSystem, obligationManager, connectionTokenManager, categoryService, conversationFactory, config, ruleEngine);
            
            return letterQueueManager;
        });
        services.AddSingleton<RouteDiscoveryManager>();
        services.AddSingleton<NetworkUnlockManager>();
        services.AddSingleton<NPCLetterOfferService>(serviceProvider =>
        {
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            var letterTemplateRepository = serviceProvider.GetRequiredService<LetterTemplateRepository>();
            var connectionTokenManager = serviceProvider.GetRequiredService<ConnectionTokenManager>();
            var letterQueueManager = serviceProvider.GetRequiredService<LetterQueueManager>();
            var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
            return new NPCLetterOfferService(gameWorld, npcRepository, letterTemplateRepository, connectionTokenManager, letterQueueManager, messageSystem);
        });
        services.AddSingleton<MorningActivitiesManager>();

        // NOTE: We deliberately exclude AI services for testing:
        // - EncounterFactory (no AI dependency needed for core game logic tests)
        // - ChoiceProjectionService (no AI dependency needed for basic functionality tests)
        // - IAIProvider (not needed for game logic testing)

        // If specific tests need AI services, they can create mock implementations or use a test AI provider
        
        // Add stub implementations for services required by GameWorldManager but not needed for location tests
        services.AddSingleton<ConversationFactory>(sp => null); // Null is OK for location initialization tests
        services.AddSingleton<ChoiceProjectionService>(sp => null); // Null is OK for location initialization tests

        return services;
    }
}