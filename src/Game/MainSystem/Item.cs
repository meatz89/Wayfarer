public enum EquipmentCategory
{
    // Required categories (hard requirements)
    Climbing_Equipment,
    Water_Transport,
    Special_Access,

    // Efficiency categories (soft modifiers)
    Navigation_Tools,
    Weather_Protection,
    Load_Distribution,
    Light_Source
}

public enum ItemCategory
{
    // Strategic categories that influence multiple systems
    Trade_Goods,        // Standard trading merchandise
    Luxury_Items,       // High-value, low-volume goods
    Bulk_Goods,         // Low-value, high-volume commodities
    Perishable,         // Time-sensitive goods
    Raw_Materials,      // Unprocessed resources
    Finished_Goods,     // Processed/manufactured items
    Information,        // Maps, documents, news
    Services            // Intangible offerings
}

public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Weight { get; set; } = 1;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int InventorySlots { get; set; } = 1;
    public List<EquipmentCategory> Categories { get; set; } = new List<EquipmentCategory>();
    public List<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();
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
    public string EquipmentCategoriesDescription
    {
        get
        {
            return Categories.Any()
        ? $"Equipment: {string.Join(", ", Categories.Select(c => c.ToString().Replace('_', ' ')))}"
        : "";
        }
    }

    public string ItemCategoriesDescription
    {
        get
        {
            return ItemCategories.Any()
        ? $"Item: {string.Join(", ", ItemCategories.Select(c => c.ToString().Replace('_', ' ')))}"
        : "";
        }
    }

    public string AllCategoriesDescription
    {
        get
        {
            List<string> descriptions = new List<string>();
            if (!string.IsNullOrEmpty(EquipmentCategoriesDescription))
                descriptions.Add(EquipmentCategoriesDescription);
            if (!string.IsNullOrEmpty(ItemCategoriesDescription))
                descriptions.Add(ItemCategoriesDescription);
            return string.Join(" • ", descriptions);
        }
    }

    // Determine if this is primarily equipment or other item types
    public bool IsPrimaryEquipment
    {
        get
        {
            return Categories.Any() && !ItemCategories.Any();
        }
    }

    public bool IsPrimaryItem
    {
        get
        {
            return ItemCategories.Any() && !Categories.Any();
        }
    }

    public bool IsHybridItem
    {
        get
        {
            return Categories.Any() && ItemCategories.Any();
        }
    }

    public bool IsAvailable { get; internal set; }
}