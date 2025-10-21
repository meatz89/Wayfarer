using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing item data from JSON.
/// Maps to the structure in items.json.
/// </summary>
public class ItemDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int InitiativeCost { get; set; } = 1;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int InventorySlots { get; set; } = 1;
    public string SizeCategory { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    public List<string> ItemCategories { get; set; } = new List<string>();
    public List<string> ToolCategories { get; set; } = new List<string>();
    public string Description { get; set; }
    public bool? IsEquipment { get; set; }
    public int? StaminaBonus { get; set; }

    // Token generation modifiers (e.g., {"Common": 1.5} for +50% Common tokens)
    public Dictionary<string, float> TokenGenerationModifiers { get; set; } = new Dictionary<string, float>();

    // Token types this equipment enables (e.g., ["Noble"] for Fine Clothes)
    public List<string> EnablesTokenGeneration { get; set; } = new List<string>();

    // Context matching system - contexts where this equipment is applicable
    public List<string> ApplicableContexts { get; set; } = new List<string>();

    // Intensity reduction when contexts match (e.g., 1 or 2)
    public int? IntensityReduction { get; set; }

    // Equipment usage type - "Permanent" (always functional), "Consumable" (single use), or "Exhaustible" (multi-use with repair)
    public string UsageType { get; set; }

    // EXHAUSTION SYSTEM (for Exhaustible equipment only)
    /// <summary>
    /// Categorical durability property - parser translates to mechanical values
    /// Values: "Fragile" (2 uses, 10 coins), "Sturdy" (5 uses, 25 coins), "Durable" (8 uses, 40 coins)
    /// </summary>
    public string Durability { get; set; }

}