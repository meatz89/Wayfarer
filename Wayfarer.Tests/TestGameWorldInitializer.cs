using Wayfarer.Game.MainSystem;
using System.IO;

namespace Wayfarer.Tests;

/// <summary>
/// Simple, synchronous initializer for creating GameWorld instances for testing.
/// Unlike the production GameWorldInitializer which loads from JSON files asynchronously,
/// this creates GameWorld objects directly with controlled, deterministic state.
/// 
/// Key Design Principles:
/// - Synchronous operation (no async complexity)
/// - Direct object creation (no JSON parsing)
/// - Deterministic state (same input = same output)
/// - Produces identical GameWorld type as production
/// </summary>
public static class TestGameWorldInitializer
{
    /// <summary>
    /// Create a simple test GameWorld with basic configuration.
    /// This provides a minimal viable game world for testing core functionality.
    /// </summary>
    public static GameWorld CreateSimpleTestWorld()
    {
        var gameWorld = new GameWorld();
        
        // Initialize with basic test data
        SetupBasicTestData(gameWorld);
        
        return gameWorld;
    }
    
    /// <summary>
    /// Create a test GameWorld from a declarative test scenario.
    /// This is the primary method for test setup - allows complete control over game state.
    /// </summary>
    public static GameWorld CreateTestWorld(TestScenarioBuilder scenario)
    {
        var gameWorld = new GameWorld();
        
        // Apply scenario configuration to game world
        scenario.ApplyToGameWorld(gameWorld);
        
        return gameWorld;
    }
    
    /// <summary>
    /// Create a test GameWorld with specific player starting conditions.
    /// Useful for tests that need specific player state without full scenario building.
    /// </summary>
    public static GameWorld CreateTestWorldWithPlayer(
        string startLocationId = "dusty_flagon",
        int coins = 50,
        int stamina = 10,
        List<string>? inventory = null)
    {
        var gameWorld = new GameWorld();
        
        // Set up basic world data
        SetupBasicTestData(gameWorld);
        
        // Configure player
        Player player = gameWorld.GetPlayer();
        player.Coins = coins;
        player.Stamina = stamina;
        
        // Add inventory items
        if (inventory != null)
        {
            foreach (string item in inventory)
            {
                player.Inventory.AddItem(item);
            }
        }
        
        // Set player location
        if (!string.IsNullOrEmpty(startLocationId))
        {
            Location? startLocation = gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == startLocationId);
            if (startLocation != null)
            {
                player.CurrentLocation = startLocation;
                gameWorld.WorldState.SetCurrentLocation(startLocation, null); // TODO: Set appropriate spot
            }
        }
        
        return gameWorld;
    }
    
    /// <summary>
    /// Setup basic test data that most tests will need.
    /// Provides minimal viable game content for testing core functionality.
    /// </summary>
    internal static void SetupBasicTestData(GameWorld gameWorld)
    {
        // Initialize collections
        gameWorld.WorldState.locations = new List<Location>();
        gameWorld.WorldState.Items = new List<Item>();
        gameWorld.WorldState.Contracts = new List<Contract>();
        
        // Add basic test locations
        gameWorld.WorldState.locations.AddRange(new[]
        {
            new Location("dusty_flagon", "The Dusty Flagon")
            {
                Description = "A rustic tavern and trading post"
            },
            new Location("town_square", "Town Square")
            {
                Description = "The bustling center of commerce"
            },
            new Location("workshop", "Artisan Workshop")
            {
                Description = "A place for skilled craftswork"
            },
            new Location("mountain_summit", "Mountain Summit")
            {
                Description = "A high peak with commanding views"
            }
        });
        
        // Load items from JSON file like production GameWorldInitializer
        List<Item> items = new List<Item>();
        string itemsFilePath = Path.Combine("Content", "Templates", "items.json");
        if (File.Exists(itemsFilePath))
        {
            items = GameWorldSerializer.DeserializeItems(
                File.ReadAllText(itemsFilePath));
            gameWorld.WorldState.Items.AddRange(items);
            Console.WriteLine($"Loaded {items.Count} items from JSON templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: items.json not found at {itemsFilePath}. Using basic test items.");
            // Fallback to basic test items
            gameWorld.WorldState.Items.AddRange(new[]
            {
                new Item
                {
                    Id = "herbs",
                    Name = "Herbs",
                    BuyPrice = 8,
                    SellPrice = 6,
                    Weight = 1,
                    Description = "Common medicinal herbs",
                    Categories = new List<EquipmentCategory>(),
                    ItemCategories = new List<ItemCategory> { ItemCategory.Trade_Goods }
                },
                new Item
                {
                    Id = "tools",
                    Name = "Tools",
                    BuyPrice = 15,
                    SellPrice = 12,
                    Weight = 3,
                    Description = "Basic craftsman tools",
                    Categories = new List<EquipmentCategory>(),
                    ItemCategories = new List<ItemCategory> { ItemCategory.Finished_Goods }
                }
            });
        }
        
        // Load contracts from JSON file like production GameWorldInitializer
        List<Contract> contracts = new List<Contract>();
        string contractsFilePath = Path.Combine("Content", "Templates", "contracts.json");
        if (File.Exists(contractsFilePath))
        {
            contracts = GameWorldSerializer.DeserializeContracts(
                File.ReadAllText(contractsFilePath));
            gameWorld.WorldState.Contracts.AddRange(contracts);
            Console.WriteLine($"Loaded {contracts.Count} contracts from JSON templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: contracts.json not found at {contractsFilePath}. Using empty contract list.");
        }
        
        // Set basic game state
        gameWorld.WorldState.CurrentDay = 1;
        gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
        gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;
        
        // Initialize player at default location
        Player player = gameWorld.GetPlayer();
        player.Initialize("Test Player", Professions.Merchant, Genders.Male);
        
        // Set starting location
        Location startLocation = gameWorld.WorldState.locations.First(l => l.Id == "dusty_flagon");
        player.CurrentLocation = startLocation;
        gameWorld.WorldState.SetCurrentLocation(startLocation, null);
        
        // Initialize empty contract list
        gameWorld.ActiveContracts = new List<Contract>();
    }
    
    /// <summary>
    /// Add a test contract to the game world.
    /// Utility method for adding contracts without full scenario building.
    /// </summary>
    public static void AddTestContract(GameWorld gameWorld, Contract contract)
    {
        if (gameWorld.WorldState.Contracts == null)
        {
            gameWorld.WorldState.Contracts = new List<Contract>();
        }
        
        gameWorld.WorldState.Contracts.Add(contract);
    }
    
    /// <summary>
    /// Add a test item to the game world.
    /// Utility method for adding items without full scenario building.
    /// </summary>
    public static void AddTestItem(GameWorld gameWorld, Item item)
    {
        if (gameWorld.WorldState.Items == null)
        {
            gameWorld.WorldState.Items = new List<Item>();
        }
        
        gameWorld.WorldState.Items.Add(item);
    }
    
    /// <summary>
    /// Add a test location to the game world.
    /// Utility method for adding locations without full scenario building.
    /// </summary>
    public static void AddTestLocation(GameWorld gameWorld, Location location)
    {
        if (gameWorld.WorldState.locations == null)
        {
            gameWorld.WorldState.locations = new List<Location>();
        }
        
        gameWorld.WorldState.locations.Add(location);
    }
}