using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Configuration data loaded from gameWorld.json
public class GameWorldConfigData
{
    public string CurrentLocationId { get; set; }
    public string CurrentLocationSpotId { get; set; }
    public PlayerInitialConfig Player { get; set; }
}

/// <summary>
/// Phase 1: Load core entities that have no dependencies on other entities.
/// This includes: GameWorld base, Locations, Items
/// </summary>
public class Phase1_CoreEntities : IInitializationPhase
{
    public int PhaseNumber => 1;
    public string Name => "Core Entities (No Dependencies)";
    public bool IsCritical => true;

    public void Execute(InitializationContext context)
    {
        // 1. Load GameWorld base configuration
        LoadGameWorldBase(context);

        // 2. Load Locations (no dependencies)
        LoadLocations(context);

        // 3. Load Items (no dependencies)
        LoadItems(context);
    }

    private void LoadGameWorldBase(InitializationContext context)
    {
        string gameWorldPath = Path.Combine(context.ContentPath, "gameWorld.json");

        if (!File.Exists(gameWorldPath))
        {
            // Create default GameWorld configuration
            Console.WriteLine("WARNING: gameWorld.json not found, using defaults");
            // These are player properties, not WorldState properties
            return;
        }

        // Actually load and parse the gameWorld.json
        string json = File.ReadAllText(gameWorldPath);
        var gameWorldData = Newtonsoft.Json.JsonConvert.DeserializeObject<GameWorldConfigData>(json);

        // Store initialization data directly in GameWorld
        // GameWorld is the single source of truth - no SharedData dictionary
        if (gameWorldData != null)
        {
            if (gameWorldData.CurrentLocationId != null)
            {
                context.GameWorld.InitialLocationId = gameWorldData.CurrentLocationId;
            }
            if (gameWorldData.CurrentLocationSpotId != null)
            {
                context.GameWorld.InitialLocationSpotId = gameWorldData.CurrentLocationSpotId;
            }
            if (gameWorldData.Player != null)
            {
                context.GameWorld.InitialPlayerConfig = gameWorldData.Player;
            }
        }

        Console.WriteLine("Loaded gameWorld.json configuration");
    }

    private void LoadLocations(InitializationContext context)
    {
        string locationsPath = Path.Combine(context.ContentPath, "locations.json");

        if (!File.Exists(locationsPath))
        {
            context.Errors.Add("locations.json not found - this is required for player initialization");
            return;
        }

        List<LocationDTO> locationDTOs = context.ContentLoader.LoadValidatedContent<List<LocationDTO>>(locationsPath);

        if (locationDTOs == null || !locationDTOs.Any())
        {
            context.Errors.Add("No locations found in locations.json");
            return;
        }

        LocationFactory locationFactory = new LocationFactory();

        foreach (LocationDTO dto in locationDTOs)
        {
            Location location = locationFactory.CreateLocation(
                dto.Id,
                dto.Name,
                dto.Description,
                dto.ConnectedTo ?? new List<string>(),
                dto.LocationSpots ?? new List<string>(),
                dto.DomainTags ?? new List<string>(),
                dto.TravelHubSpotId,
                null, // environmentalProperties - TODO: convert from DTO
                null, // availableProfessionsByTime - TODO: convert from DTO
                dto.Tier
            );

            context.GameWorld.WorldState.locations.Add(location);
            Console.WriteLine($"  Loaded location: {location.Id} - {location.Name}");
        }

        Console.WriteLine($"Loaded {context.GameWorld.WorldState.locations.Count} locations");
    }

    private void LoadItems(InitializationContext context)
    {
        string itemsPath = Path.Combine(context.ContentPath, "items.json");

        if (!File.Exists(itemsPath))
        {
            Console.WriteLine("INFO: items.json not found, skipping item loading");
            return;
        }

        List<ItemDTO> itemDTOs = context.ContentLoader.LoadValidatedContent<List<ItemDTO>>(itemsPath);

        if (itemDTOs == null || !itemDTOs.Any())
        {
            Console.WriteLine("WARNING: No items found in items.json");
            return;
        }

        ItemFactory itemFactory = new ItemFactory();

        foreach (ItemDTO dto in itemDTOs)
        {
            // Parse size category
            SizeCategory size = SizeCategory.Small;
            if (!string.IsNullOrEmpty(dto.SizeCategory))
            {
                if (!Enum.TryParse<SizeCategory>(dto.SizeCategory, true, out size))
                {
                    context.Warnings.Add($"Invalid size category '{dto.SizeCategory}' for item {dto.Id}, defaulting to Small");
                }
            }

            // Parse item categories
            List<ItemCategory> categories = new List<ItemCategory>();
            foreach (string catStr in dto.Categories ?? new List<string>())
            {
                if (Enum.TryParse<ItemCategory>(catStr, true, out ItemCategory category))
                {
                    categories.Add(category);
                }
            }

            Item item = new Item
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description ?? "",
                BuyPrice = dto.BuyPrice,
                SellPrice = dto.SellPrice,
                Weight = dto.Weight,
                InventorySlots = dto.InventorySlots,
                Size = size,
                Categories = categories,
            };

            // Parse token generation modifiers
            if (dto.TokenGenerationModifiers != null && dto.TokenGenerationModifiers.Any())
            {
                foreach (KeyValuePair<string, float> modifier in dto.TokenGenerationModifiers)
                {
                    if (Enum.TryParse<ConnectionType>(modifier.Key, true, out ConnectionType tokenType))
                    {
                        item.TokenGenerationModifiers[tokenType] = modifier.Value;
                    }
                    else
                    {
                        context.Warnings.Add($"Invalid token type '{modifier.Key}' in modifiers for item {dto.Id}");
                    }
                }
            }

            // Parse enabled token types
            if (dto.EnablesTokenGeneration != null && dto.EnablesTokenGeneration.Any())
            {
                foreach (string tokenTypeStr in dto.EnablesTokenGeneration)
                {
                    if (Enum.TryParse<ConnectionType>(tokenTypeStr, true, out ConnectionType tokenType))
                    {
                        item.EnablesTokenGeneration.Add(tokenType);
                    }
                    else
                    {
                        context.Warnings.Add($"Invalid token type '{tokenTypeStr}' in EnablesTokenGeneration for item {dto.Id}");
                    }
                }
            }

            context.GameWorld.WorldState.Items.Add(item);

            // Validate price logic
            if (dto.SellPrice > dto.BuyPrice)
            {
                context.Warnings.Add($"Item '{dto.Name}' has sell price ({dto.SellPrice}) higher than buy price ({dto.BuyPrice}). This may create infinite money exploits.");
            }
        }

        Console.WriteLine($"Loaded {context.GameWorld.WorldState.Items.Count} items");
    }
}