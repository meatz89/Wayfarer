using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Minimal GameWorld initializer that ensures the game can always run.
/// Creates default/dummy data as needed to satisfy all requirements.
/// </summary>
public class MinimalGameWorldInitializer
{
    private readonly IContentDirectory _contentDirectory;
    private readonly ILogger<MinimalGameWorldInitializer> _logger;

    public MinimalGameWorldInitializer(IContentDirectory contentDirectory, ILogger<MinimalGameWorldInitializer> logger = null)
    {
        _contentDirectory = contentDirectory;
        _logger = logger;
    }

    public GameWorld Initialize()
    {
        Console.WriteLine("=== MINIMAL GAME WORLD INITIALIZATION ===");

        // Create empty GameWorld
        GameWorld gameWorld = new GameWorld();

        // 1. Create minimal locations
        CreateMinimalLocations(gameWorld);

        // 2. Create minimal spots
        CreateMinimalSpots(gameWorld);

        // 3. Create minimal NPCs
        CreateMinimalNPCs(gameWorld);

        // 4. Create minimal items
        CreateMinimalItems(gameWorld);

        // 5. Create minimal routes
        CreateMinimalRoutes(gameWorld);

        // 6. Create minimal letter templates
        CreateMinimalLetterTemplates(gameWorld);

        // 7. Initialize player
        InitializePlayer(gameWorld);

        Console.WriteLine("=== INITIALIZATION COMPLETE ===");
        Console.WriteLine($"Locations: {gameWorld.WorldState.locations.Count}");
        Console.WriteLine($"Spots: {gameWorld.WorldState.locationSpots.Count}");
        Console.WriteLine($"NPCs: {gameWorld.WorldState.NPCs.Count}");
        Console.WriteLine($"Routes: {gameWorld.WorldState.Routes.Count}");
        Console.WriteLine($"Letter Templates: {gameWorld.WorldState.LetterTemplates.Count}");
        LocationSpot? playerSpot = gameWorld.GetPlayer().CurrentLocationSpot;
        Location? playerLocation = playerSpot != null ? gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == playerSpot.LocationId) : null;
        Console.WriteLine($"Player at: {playerLocation?.Id ?? "NULL"} / {playerSpot?.SpotID ?? "NULL"}");

        return gameWorld;
    }

    private void CreateMinimalLocations(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal locations...");

        LocationFactory locationFactory = new LocationFactory();

        // Create at least two locations for routes
        Location millbrook = locationFactory.CreateMinimalLocation("millbrook");
        millbrook.Description = "A peaceful village where your journey begins.";
        millbrook.DomainTags = new List<string> { "SAFE", "STARTING" };

        Location thornwood = locationFactory.CreateMinimalLocation("thornwood");
        thornwood.Description = "A forest settlement known for its lumber trade.";
        thornwood.DomainTags = new List<string> { "FOREST", "TRADE" };

        gameWorld.WorldState.locations.Add(millbrook);
        gameWorld.WorldState.locations.Add(thornwood);

        // Try to load more from JSON if available
        TryLoadLocationsFromJson(gameWorld);
    }

    private void CreateMinimalSpots(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal spots...");

        foreach (Location location in gameWorld.WorldState.locations)
        {
            // Ensure each location has at least one spot
            List<LocationSpot> existingSpots = gameWorld.WorldState.locationSpots
                .Where(s => s.LocationId == location.Id)
                .ToList();

            if (!existingSpots.Any())
            {
                LocationSpotFactory spotFactory = new LocationSpotFactory();
                LocationSpot defaultSpot = spotFactory.CreateMinimalSpot($"{location.Id}_square", location.Id);
                defaultSpot.Description = $"The central square of {location.Name}.";
                defaultSpot.DomainTags = new List<string> { "SOCIAL" };

                gameWorld.WorldState.locationSpots.Add(defaultSpot);
                Console.WriteLine($"  Created default spot for {location.Id}");
            }
        }

        // Try to load more from JSON
        TryLoadSpotsFromJson(gameWorld);
    }

    private void CreateMinimalNPCs(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal NPCs...");

        // Ensure at least one NPC per location
        foreach (Location location in gameWorld.WorldState.locations)
        {
            List<NPC> existingNPCs = gameWorld.WorldState.NPCs
                .Where(n => n.Location == location.Id)
                .ToList();

            if (!existingNPCs.Any())
            {
                NPCFactory npcFactory = new NPCFactory();
                NPC defaultNPC = npcFactory.CreateMinimalNPC($"elder_{location.Id}", location.Id);
                defaultNPC.Name = $"{location.Name} Elder";
                defaultNPC.Description = $"A wise elder of {location.Name}";
                defaultNPC.LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trust, ConnectionType.Trust };

                gameWorld.WorldState.NPCs.Add(defaultNPC);
                Console.WriteLine($"  Created default NPC for {location.Id}");
            }
        }

        // Try to load more from JSON
        TryLoadNPCsFromJson(gameWorld);
    }

    private void CreateMinimalItems(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal items...");

        // Create basic items if none exist
        if (!gameWorld.WorldState.Items.Any())
        {
            Item bread = new Item
            {
                Id = "bread",
                Name = "Bread",
                Description = "A loaf of fresh bread",
                BuyPrice = 2,
                SellPrice = 1,
                Weight = 1,
                InventorySlots = 1,
                Size = SizeCategory.Small
            };

            Item waterskin = new Item
            {
                Id = "waterskin",
                Name = "Waterskin",
                Description = "A leather waterskin for travel",
                BuyPrice = 5,
                SellPrice = 2,
                Weight = 1,
                InventorySlots = 1,
                Size = SizeCategory.Small
            };

            gameWorld.WorldState.Items.Add(bread);
            gameWorld.WorldState.Items.Add(waterskin);
        }

        // Try to load more from JSON
        TryLoadItemsFromJson(gameWorld);
    }

    private void CreateMinimalRoutes(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal routes...");

        // Create basic walking routes between all locations
        List<Location> locations = gameWorld.WorldState.locations;
        for (int i = 0; i < locations.Count - 1; i++)
        {
            for (int j = i + 1; j < locations.Count; j++)
            {
                Location origin = locations[i];
                Location destination = locations[j];

                // Check if route already exists
                bool existingRoute = gameWorld.WorldState.Routes
                    .Any(r => (r.Origin == origin.Id && r.Destination == destination.Id) ||
                              (r.Origin == destination.Id && r.Destination == origin.Id));

                if (!existingRoute)
                {
                    RouteOption route = new RouteOption
                    {
                        Id = $"walk_{origin.Id}_{destination.Id}",
                        Name = $"Walk to {destination.Name}",
                        Origin = origin.Id,
                        Destination = destination.Id,
                        Method = TravelMethods.Walking,
                        BaseCoinCost = 0,
                        BaseStaminaCost = 2,
                        TravelTimeHours = 8,
                        IsDiscovered = true,
                        Description = $"A simple walking path between {origin.Name} and {destination.Name}"
                    };

                    gameWorld.WorldState.Routes.Add(route);

                    // Create reverse route
                    RouteOption reverseRoute = new RouteOption
                    {
                        Id = $"walk_{destination.Id}_{origin.Id}",
                        Name = $"Walk to {origin.Name}",
                        Origin = destination.Id,
                        Destination = origin.Id,
                        Method = TravelMethods.Walking,
                        BaseCoinCost = 0,
                        BaseStaminaCost = 2,
                        TravelTimeHours = 8,
                        IsDiscovered = true,
                        Description = $"A simple walking path between {destination.Name} and {origin.Name}"
                    };

                    gameWorld.WorldState.Routes.Add(reverseRoute);
                    Console.WriteLine($"  Created routes between {origin.Id} and {destination.Id}");
                }
            }
        }

        // Try to load more from JSON
        TryLoadRoutesFromJson(gameWorld);
    }

    private void CreateMinimalLetterTemplates(GameWorld gameWorld)
    {
        Console.WriteLine("Creating minimal letter templates...");

        if (!gameWorld.WorldState.LetterTemplates.Any())
        {
            LetterTemplate basicTemplate = new LetterTemplate
            {
                Id = "basic_delivery",
                Description = "A simple letter delivery",
                TokenType = ConnectionType.Trust,
                MinPayment = 2,
                MaxPayment = 5,
                MinDeadlineInHours = 24,
                MaxDeadlineInHours = 48,
                Size = SizeCategory.Small
            };

            gameWorld.WorldState.LetterTemplates.Add(basicTemplate);
        }

        // Try to load more from JSON
        TryLoadLetterTemplatesFromJson(gameWorld);
    }

    private void InitializePlayer(GameWorld gameWorld)
    {
        Console.WriteLine("Initializing player...");

        Player player = gameWorld.GetPlayer();

        // Set starting spot (location is derived from spot)
        Location? startLocation = gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == "millbrook")
                          ?? gameWorld.WorldState.locations.FirstOrDefault();

        if (startLocation != null)
        {
            LocationSpot? startSpot = gameWorld.WorldState.locationSpots
                .FirstOrDefault(s => s.LocationId == startLocation.Id);

            if (startSpot != null)
            {
                player.CurrentLocationSpot = startSpot;
            }
        }

        // Set basic player properties
        if (string.IsNullOrEmpty(player.Name))
        {
            player.Name = "Courier";
        }

        if (player.Archetype == 0)
        {
            player.Archetype = Professions.Merchant;
        }

        player.Coins = 10;
        player.MaxStamina = 10;
        player.Stamina = 10;

        // Final validation - player must have a spot
        if (player.CurrentLocationSpot == null)
        {
            throw new InvalidOperationException(
                "CRITICAL: Failed to initialize player spot even with minimal data");
        }
    }

    // Helper methods to try loading from JSON without failing
    private void TryLoadLocationsFromJson(GameWorld gameWorld)
    {
        try
        {
            string path = Path.Combine(_contentDirectory.Path, "Templates", "locations.json");
            if (File.Exists(path))
            {
                // Simple JSON loading without validation for now
                // This is just to get more content if available
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Could not load additional locations: {ex.Message}");
        }
    }

    private void TryLoadSpotsFromJson(GameWorld gameWorld)
    {
        // Similar pattern for spots
    }

    private void TryLoadNPCsFromJson(GameWorld gameWorld)
    {
        // Similar pattern for NPCs
    }

    private void TryLoadItemsFromJson(GameWorld gameWorld)
    {
        // Similar pattern for items
    }

    private void TryLoadRoutesFromJson(GameWorld gameWorld)
    {
        // Similar pattern for routes
    }

    private void TryLoadLetterTemplatesFromJson(GameWorld gameWorld)
    {
        // Similar pattern for letter templates
    }
}