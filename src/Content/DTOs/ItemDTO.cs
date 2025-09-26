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

}