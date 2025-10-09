

public enum ItemCategory
{
    // Equipment types
    Climbing_Equipment, // Ropes, grappling hooks, pitons
    Water_Transport,    // Boats, rafts, swimming gear
    Navigation_Tools,   // Maps, compasses
    Weather_Protection, // Cloaks, boots, umbrellas
    Load_Distribution,  // Backpacks, carts, bags
    Light_Source,       // Lanterns, torches, candles
    Special_Access,     // Permits, official documents, keys

    // Other item types
    Hunger,              // Bread, meat, water
    Documents,         // Letters, contracts, books
    Tools,             // Hammers, saws, needles
    Weapons,           // Swords, bows, shields
    Clothing,          // Shirts, pants, hats
    Medicine,          // Herbs, potions, bandages
    Valuables,         // Jewelry, gems, coins
    Materials,         // Wood, metal, cloth
    Luxury_Items,      // Expensive specialty items
    Trade_Goods,       // Basic trade commodities
    Equipment,         // General equipment category
    Special_Document   // Readable letters and important documents
}

public enum SizeCategory
{
    Tiny,      // Weight 1 - Fits in pocket (letters, permits, coins)
    Small,     // Weight 1-2 - Single hand carry (consumables, small packages)
    Medium,    // Weight 2-3 - Two-handed carry (tools, trade goods)
    Large,     // Weight 3-5 - Requires effort (heavy packages, bulk goods)
    Massive    // Weight 5-6 - Maximum burden (crates, heavy deliveries)
}

public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int InitiativeCost { get; set; } = 1;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public List<ItemCategory> Categories { get; set; } = new List<ItemCategory>();

    // Weight system - replaces slots (1-6 weight scale)
    public int Weight { get; set; } = 1;

    // Enhanced Categorical Properties
    public SizeCategory Size { get; set; } = SizeCategory.Medium;

    public string VenueId { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }

    // Token generation modifiers for equipment
    public Dictionary<ConnectionType, float> TokenGenerationModifiers { get; set; } = new Dictionary<ConnectionType, float>();

    // Token types this equipment enables (e.g., Fine Clothes enables Noble token generation)
    public List<ConnectionType> EnablesTokenGeneration { get; set; } = new List<ConnectionType>();

    // Equipment Category System - Categories this equipment provides for tactical card requirements
    public List<EquipmentCategory> ProvidedEquipmentCategories { get; set; } = new List<EquipmentCategory>();

    // Equipment Durability System - Physical engagements can degrade equipment
    public int Durability { get; set; } = 100;
    public int MaxDurability { get; set; } = 100;
    public bool IsDegraded => Durability < MaxDurability;

    public string InitiativeCostDescription => InitiativeCost switch
    {
        0 => "InitiativeCostless",
        1 => "Light",
        2 => "Medium",
        3 => "Heavy",
        _ => $"Very Heavy ({InitiativeCost})"
    };

    // Helper properties for UI display
    public string CategoriesDescription => Categories.Any()
        ? $"Categories: {string.Join(", ", Categories.Select(c => c.ToString().Replace('_', ' ')))}"
        : "";



    public string SizeCategoryDescription => $"Size: {Size.ToString()}";
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



    public bool IsEquipment => Categories.Any(c =>
                                            c == ItemCategory.Climbing_Equipment ||
                                            c == ItemCategory.Water_Transport ||
                                            c == ItemCategory.Special_Access ||
                                            c == ItemCategory.Navigation_Tools ||
                                            c == ItemCategory.Weather_Protection ||
                                            c == ItemCategory.Load_Distribution ||
                                            c == ItemCategory.Light_Source);


    // Categorical matching helper methods
    public bool HasCategory(ItemCategory category)
    {
        return Categories.Contains(category);
    }



    public bool IsAvailable { get; internal set; }

    /// <summary>
    /// Get the weight of this item. If not explicitly set, derives from size category.
    /// </summary>
    public int GetWeight()
    {
        // If weight explicitly set (>0), use that
        if (Weight > 0) return Weight;

        // Otherwise derive from size category
        return Size switch
        {
            SizeCategory.Tiny => 1,
            SizeCategory.Small => 2,
            SizeCategory.Medium => 3,
            SizeCategory.Large => 4,
            SizeCategory.Massive => 5,
            _ => 1
        };
    }





}