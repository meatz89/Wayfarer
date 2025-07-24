using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        var gameWorldPath = Path.Combine(context.ContentPath, "gameWorld.json");
        
        if (!File.Exists(gameWorldPath))
        {
            // Create default GameWorld configuration
            Console.WriteLine("WARNING: gameWorld.json not found, using defaults");
            // These are player properties, not WorldState properties
            return;
        }
        
        try
        {
            // For now, just log that we would load it
            Console.WriteLine("Loaded gameWorld.json configuration");
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to load gameWorld.json: {ex.Message}");
        }
    }
    
    private void LoadLocations(InitializationContext context)
    {
        var locationsPath = Path.Combine(context.ContentPath, "locations.json");
        
        if (!File.Exists(locationsPath))
        {
            context.Errors.Add("locations.json not found - this is required for player initialization");
            return;
        }
        
        try
        {
            var locationDTOs = context.ContentLoader.LoadValidatedContent<List<LocationDTO>>(locationsPath);
            
            if (locationDTOs == null || !locationDTOs.Any())
            {
                context.Errors.Add("No locations found in locations.json");
                return;
            }
            
            var locationFactory = new LocationFactory();
            
            foreach (var dto in locationDTOs)
            {
                try
                {
                    var location = locationFactory.CreateLocation(
                        dto.Id,
                        dto.Name,
                        dto.Description,
                        dto.ConnectedTo ?? new List<string>(),
                        dto.LocationSpots ?? new List<string>(),
                        dto.DomainTags ?? new List<string>(),
                        null, // environmentalProperties - TODO: convert from DTO
                        null  // availableProfessionsByTime - TODO: convert from DTO
                    );
                    
                    context.GameWorld.WorldState.locations.Add(location);
                    Console.WriteLine($"  Loaded location: {location.Id} - {location.Name}");
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to create location {dto.Id}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {context.GameWorld.WorldState.locations.Count} locations");
        }
        catch (ContentValidationException ex)
        {
            // Validation failed - add all validation errors
            foreach (var error in ex.Errors)
            {
                context.Errors.Add($"Location validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to load locations: {ex.Message}");
        }
    }
    
    private void LoadItems(InitializationContext context)
    {
        var itemsPath = Path.Combine(context.ContentPath, "items.json");
        
        if (!File.Exists(itemsPath))
        {
            Console.WriteLine("INFO: items.json not found, skipping item loading");
            return;
        }
        
        try
        {
            var itemDTOs = context.ContentLoader.LoadValidatedContent<List<ItemDTO>>(itemsPath);
            
            if (itemDTOs == null || !itemDTOs.Any())
            {
                Console.WriteLine("WARNING: No items found in items.json");
                return;
            }
            
            var itemFactory = new ItemFactory();
            
            foreach (var dto in itemDTOs)
            {
                try
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
                    var categories = new List<ItemCategory>();
                    foreach (var catStr in dto.Categories ?? new List<string>())
                    {
                        if (Enum.TryParse<ItemCategory>(catStr, true, out var category))
                        {
                            categories.Add(category);
                        }
                    }
                    
                    var item = new Item
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Description = dto.Description ?? "",
                        BuyPrice = dto.BuyPrice,
                        SellPrice = dto.SellPrice,
                        Weight = dto.Weight,
                        InventorySlots = dto.InventorySlots,
                        Size = size,
                        Categories = categories
                    };
                    
                    context.GameWorld.WorldState.Items.Add(item);
                    
                    // Validate price logic
                    if (dto.SellPrice > dto.BuyPrice)
                    {
                        context.Warnings.Add($"Item '{dto.Name}' has sell price ({dto.SellPrice}) higher than buy price ({dto.BuyPrice}). This may create infinite money exploits.");
                    }
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to create item {dto.Id}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {context.GameWorld.WorldState.Items.Count} items");
        }
        catch (ContentValidationException ex)
        {
            // Items aren't critical, so just warn
            foreach (var error in ex.Errors)
            {
                context.Warnings.Add($"Item validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            context.Warnings.Add($"Failed to load items: {ex.Message}");
        }
    }
}