

public enum ItemCategory
{
    // Equipment types
    Climbing_Equipment, // Ropes, grappling hooks, pitons
    Water_Transport,    // Boats, rafts, swimming gear
    Special_Access,     // Keys, passes, credentials
    Navigation_Tools,   // Maps, compasses
    Weather_Protection, // Cloaks, boots, umbrellas
    Load_Distribution,  // Backpacks, carts, bags
    Light_Source,       // Lanterns, torches, candles
    
    // Other item types
    Food,              // Bread, meat, water
    Documents,         // Letters, contracts, books
    Tools,             // Hammers, saws, needles
    Weapons,           // Swords, bows, shields
    Clothing,          // Shirts, pants, hats
    Medicine,          // Herbs, potions, bandages
    Valuables,         // Jewelry, gems, coins
    Materials,         // Wood, metal, cloth
    Bulk_Goods,        // Large trade goods, cargo
    Luxury_Items,      // Expensive specialty items
    Trade_Goods,       // Basic trade commodities
    Equipment          // General equipment category
}

public enum SizeCategory
{
    Tiny,      // Fits in pocket, no transport concerns (1 slot)
    Small,     // Single hand carry, minimal impact (1 slot)
    Medium,    // Two-handed carry, some transport consideration (1 slot)
    Large,     // Requires special transport planning or stamina cost (2 slots)
    Massive    // Blocks route access without proper transport (3 slots)
}

public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Weight { get; set; } = 1;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int InventorySlots { get; set; } = 1;
    public List<ItemCategory> Categories { get; set; } = new List<ItemCategory>();

    // Enhanced Categorical Properties
    public SizeCategory Size { get; set; } = SizeCategory.Medium;

    public string LocationId { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }

    public string WeightDescription
    {
        get
        {
            return Weight switch
            {
                0 => "Weightless",
                1 => "Light",
                2 => "Medium",
                3 => "Heavy",
                _ => $"Very Heavy ({Weight})"
            };
        }
    }

    // Helper properties for UI display
    public string CategoriesDescription
    {
        get
        {
            return Categories.Any()
        ? $"Categories: {string.Join(", ", Categories.Select(c => c.ToString().Replace('_', ' ')))}"
        : "";
        }
    }



    public string SizeCategoryDescription
    {
        get
        {
            return $"Size: {Size.ToString()}";
        }
    }
    public string AllCategoriesDescription
    {
        get
        {
            List<string> descriptions = new List<string>();
            if (!string.IsNullOrEmpty(CategoriesDescription))
                descriptions.Add(CategoriesDescription);
            descriptions.Add(SizeCategoryDescription);
            return string.Join(" • ", descriptions);
        }
    }



    public bool IsEquipment
    {
        get
        {
            return Categories.Any(c => 
                c == ItemCategory.Climbing_Equipment ||
                c == ItemCategory.Water_Transport ||
                c == ItemCategory.Special_Access ||
                c == ItemCategory.Navigation_Tools ||
                c == ItemCategory.Weather_Protection ||
                c == ItemCategory.Load_Distribution ||
                c == ItemCategory.Light_Source);
        }
    }


    // Categorical matching helper methods
    public bool HasCategory(ItemCategory category)
    {
        return Categories.Contains(category);
    }


    public bool IsSizeCategory(SizeCategory sizeCategory)
    {
        return Size == sizeCategory;
    }

    public bool IsAvailable { get; internal set; }

    /// <summary>
    /// Get the number of inventory slots this item requires based on its size category
    /// </summary>
    public int GetRequiredSlots()
    {
        return Size switch
        {
            SizeCategory.Tiny => 1,
            SizeCategory.Small => 1,
            SizeCategory.Medium => 1,
            SizeCategory.Large => 2,
            SizeCategory.Massive => 3,
            _ => 1
        };
    }

    /// <summary>
    /// Check if this item is considered heavy for transport restrictions
    /// </summary>
    public bool IsHeavyForTransport()
    {
        return Size == SizeCategory.Large || Size == SizeCategory.Massive;
    }
}