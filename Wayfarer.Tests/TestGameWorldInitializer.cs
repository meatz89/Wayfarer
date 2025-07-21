using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

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
        GameWorld gameWorld = new GameWorld();

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
        // Load test content from JSON files
        // Create factories needed for GameWorldInitializer
        var locationFactory = new LocationFactory();
        var locationSpotFactory = new LocationSpotFactory();
        var npcFactory = new NPCFactory();
        var itemFactory = new ItemFactory();
        var routeFactory = new RouteFactory();
        var routeDiscoveryFactory = new RouteDiscoveryFactory();
        var networkUnlockFactory = new NetworkUnlockFactory();
        var letterTemplateFactory = new LetterTemplateFactory();
        var standingObligationFactory = new StandingObligationFactory();
        
        var contentDirectory = new ContentDirectory { Path = "Content" };
        GameWorldInitializer initializer = new GameWorldInitializer(
            contentDirectory,
            locationFactory,
            locationSpotFactory,
            npcFactory,
            itemFactory,
            routeFactory,
            routeDiscoveryFactory,
            networkUnlockFactory,
            letterTemplateFactory,
            standingObligationFactory);
        GameWorld gameWorld = initializer.LoadGame();

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
        GameWorld gameWorld = new GameWorld();

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
        gameWorld.WorldState.locationSpots = new List<LocationSpot>();
        gameWorld.WorldState.Routes = new List<RouteOption>();
        gameWorld.WorldState.Items = new List<Item>();

        // Load locations from TEST-SPECIFIC JSON file - NEVER use production content
        List<Location> locations = new List<Location>();
        string testLocationsFilePath = Path.Combine("Content", "Templates", "locations.json");
        if (File.Exists(testLocationsFilePath))
        {
            locations = GameWorldSerializer.DeserializeLocations(
                File.ReadAllText(testLocationsFilePath));
            gameWorld.WorldState.locations.AddRange(locations);
            Console.WriteLine($"Loaded {locations.Count} locations from TEST templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: TEST locations.json not found at {testLocationsFilePath}. Using basic test locations.");
            // Fallback to basic test locations
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
        }

        // Load location spots from TEST-SPECIFIC JSON file
        List<LocationSpot> locationSpots = new List<LocationSpot>();
        string testLocationSpotsFilePath = Path.Combine("Content", "Templates", "location_spots.json");
        if (File.Exists(testLocationSpotsFilePath))
        {
            locationSpots = GameWorldSerializer.DeserializeLocationSpots(
                File.ReadAllText(testLocationSpotsFilePath));
            gameWorld.WorldState.locationSpots.AddRange(locationSpots);
            Console.WriteLine($"Loaded {locationSpots.Count} location spots from TEST templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: TEST location_spots.json not found at {testLocationSpotsFilePath}. Using basic test spots.");
        }

        // Load routes from TEST-SPECIFIC JSON file
        List<RouteOption> routes = new List<RouteOption>();
        string testRoutesFilePath = Path.Combine("Content", "Templates", "routes.json");
        if (File.Exists(testRoutesFilePath))
        {
            routes = GameWorldSerializer.DeserializeRouteOptions(
                File.ReadAllText(testRoutesFilePath));
            gameWorld.WorldState.Routes.AddRange(routes);
            Console.WriteLine($"Loaded {routes.Count} routes from TEST templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: TEST routes.json not found at {testRoutesFilePath}. Using basic test routes.");
        }

        // Load items from TEST-SPECIFIC JSON file - NEVER use production content
        List<Item> items = new List<Item>();
        string testItemsFilePath = Path.Combine("Content", "Templates", "items.json");
        if (File.Exists(testItemsFilePath))
        {
            items = GameWorldSerializer.DeserializeItems(
                File.ReadAllText(testItemsFilePath));
            gameWorld.WorldState.Items.AddRange(items);
            Console.WriteLine($"Loaded {items.Count} items from TEST templates.");
        }
        else
        {
            Console.WriteLine($"WARNING: TEST items.json not found at {testItemsFilePath}. Using basic test items.");
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
                    Categories = new List<ItemCategory> { ItemCategory.Materials }
                },
                new Item
                {
                    Id = "tools",
                    Name = "Tools",
                    BuyPrice = 15,
                    SellPrice = 12,
                    Weight = 3,
                    Description = "Basic craftsman tools",
                    Categories = new List<ItemCategory> { ItemCategory.Tools }
                }
            });
        }


        // Add basic NPCs for market functionality using repository
        // These NPCs are needed for market operations to work with the scheduling system
        NPCRepository npcRepository = new NPCRepository(gameWorld);

        npcRepository.AddNPC(new NPC
        {
            ID = "marcus_innkeeper",
            Name = "Marcus the Innkeeper",
            Location = "dusty_flagon",
            Profession = Professions.Merchant,
            AvailabilitySchedule = Schedule.Always,
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.Rest }
        });

        npcRepository.AddNPC(new NPC
        {
            ID = "elena_trader",
            Name = "Elena the Trader",
            Location = "town_square",
            Profession = Professions.Merchant,
            AvailabilitySchedule = Schedule.Market_Hours,
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade }
        });

        npcRepository.AddNPC(new NPC
        {
            ID = "workshop_artisan",
            Name = "Guild Artisan",
            Location = "workshop",
            Profession = Professions.Merchant,
            AvailabilitySchedule = Schedule.Market_Hours,
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.EquipmentRepair }
        });

        npcRepository.AddNPC(new NPC
        {
            ID = "millbrook_merchant",
            Name = "Village Merchant",
            Location = "millbrook",
            Profession = Professions.Merchant,
            AvailabilitySchedule = Schedule.Market_Hours,
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade }
        });

        // Set basic game state
        gameWorld.WorldState.CurrentDay = 1;
        gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
        gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;

        // Initialize player at default location
        Player player = gameWorld.GetPlayer();
        player.Initialize("Test Player", Professions.Merchant, Genders.Male);

        // Set starting location - use the first available location from test data
        Location startLocation = gameWorld.WorldState.locations.FirstOrDefault();
        if (startLocation == null)
        {
            // If no locations loaded from test data, create a minimal one
            startLocation = new Location("test_location", "Test Location");
            var locationRepo = new LocationRepository(gameWorld);
            locationRepo.AddLocation(startLocation);
        }
        player.CurrentLocation = startLocation;
        gameWorld.WorldState.SetCurrentLocation(startLocation, null);

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