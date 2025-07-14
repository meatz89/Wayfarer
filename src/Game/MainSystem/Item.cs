using Wayfarer.Game.ActionSystem;

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
    public List<EquipmentCategory> Categories { get; set; } = new List<EquipmentCategory>();
    public List<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();

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
            if (!string.IsNullOrEmpty(EquipmentCategoriesDescription))
                descriptions.Add(EquipmentCategoriesDescription);
            if (!string.IsNullOrEmpty(ItemCategoriesDescription))
                descriptions.Add(ItemCategoriesDescription);
            descriptions.Add(SizeCategoryDescription);
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

    // Categorical matching helper methods
    public bool HasEquipmentCategory(EquipmentCategory equipmentCategory)
    {
        return Categories.Contains(equipmentCategory);
    }

    public bool HasToolCategory(ToolCategory toolCategory)
    {
        // ToolCategory now represents non-equipment tool needs
        // This would be used for checking if item can fulfill general tool requirements
        // For now, return false since items don't directly map to these general categories
        return false;
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