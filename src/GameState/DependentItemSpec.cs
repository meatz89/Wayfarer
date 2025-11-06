/// <summary>
/// Specification for generating a dependent item as part of self-contained scene
/// Scene defines these specs at authoring time, SceneInstantiator generates actual ItemDTOs at runtime
/// Generated items flow through standard JSON → DTO → Domain pipeline
/// </summary>
public class DependentItemSpec
{
    /// <summary>
    /// Unique identifier for this resource specification within the scene
    /// Used to reference this item in situation rewards and requirements
    /// Converted to marker format: "generated:{TemplateId}" for reference resolution
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Pattern for generating item name with placeholder replacement
    /// Placeholders replaced at scene finalization by PlaceholderReplacer
    /// Supported: {NPCName}, {LocationName}, {PlayerName}, {RouteName}
    /// Example: "Room Key", "{NPCName}'s Private Key", "{LocationName} Access Pass"
    /// </summary>
    public string NamePattern { get; set; }

    /// <summary>
    /// Pattern for generating item description with placeholder replacement
    /// Placeholders replaced at scene finalization by PlaceholderReplacer
    /// Supported: {NPCName}, {LocationName}, {PlayerName}, {RouteName}
    /// Example: "A key that unlocks {NPCName}'s private quarters."
    /// </summary>
    public string DescriptionPattern { get; set; }

    /// <summary>
    /// Item categories that define what this item can be used for
    /// Special_Access for keys and permits, Documents for readable items, etc.
    /// </summary>
    public List<ItemCategory> Categories { get; set; } = new List<ItemCategory>();

    /// <summary>
    /// Item weight (1-6 scale)
    /// 1 = Tiny (keys, permits), 2-3 = Small/Medium, 4-5 = Large, 6 = Massive
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Purchase price if item can be bought
    /// 0 = Cannot be purchased (most keys and special items)
    /// </summary>
    public int BuyPrice { get; set; } = 0;

    /// <summary>
    /// Selling price if item can be sold
    /// Usually 0 for key items that are quest-critical
    /// </summary>
    public int SellPrice { get; set; } = 0;

    /// <summary>
    /// Whether this item should be added directly to player inventory
    /// true = Item spawns in player's possession (common for keys received as rewards)
    /// false = Item spawns at location, player must collect it
    /// </summary>
    public bool AddToInventoryOnCreation { get; set; } = false;

    /// <summary>
    /// Location template ID where item should spawn if not added to inventory
    /// References DependentLocationSpec by TemplateId, or null for base location
    /// </summary>
    public string SpawnLocationTemplateId { get; set; }
}
