/// <summary>
/// DTO for DependentItemSpec - declarative specification for items that scenes create dynamically
/// Self-contained pattern: Scene generates JSON package with these specs → PackageLoader → Standard parsing
/// Maps to DependentItemSpec domain entity
/// </summary>
public class DependentItemSpecDTO
{
    /// <summary>
    /// Template identifier used in marker references
    /// Example: "room_key" becomes marker "generated:room_key"
    /// Also used to construct actual ID: "{sceneId}_{templateId}"
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Name pattern with {placeholders}
    /// Example: "Room Key"
    /// Placeholders replaced at finalization with actual entity names
    /// </summary>
    public string NamePattern { get; set; }

    /// <summary>
    /// Description pattern with {placeholders}
    /// Example: "A key that unlocks access to {npc_name}'s private {service_type} room."
    /// Placeholders replaced at finalization with actual entity names
    /// </summary>
    public string DescriptionPattern { get; set; }

    /// <summary>
    /// Item categories for classification
    /// Example: ["Special_Access"]
    /// Maps to ItemCategory enum values
    /// </summary>
    public List<string> Categories { get; set; }

    /// <summary>
    /// Item weight (inventory slots consumed)
    /// Default: 1
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Purchase price from merchants
    /// 0 = cannot be purchased
    /// Default: 0
    /// </summary>
    public int BuyPrice { get; set; } = 0;

    /// <summary>
    /// Sale price to merchants
    /// 0 = cannot be sold
    /// Default: 0
    /// </summary>
    public int SellPrice { get; set; } = 0;

    /// <summary>
    /// Whether to add item to player inventory immediately on creation
    /// true: Item granted when scene finalized (access key pattern)
    /// false: Item spawned in world, must be found/purchased
    /// Defaults to false if not specified
    /// </summary>
    public bool AddToInventoryOnCreation { get; set; } = false;

    /// <summary>
    /// Template ID of location where item spawns
    /// References DependentLocationSpec by TemplateId
    /// Becomes marker reference: "generated:{SpawnLocationTemplateId}"
    /// null = add to inventory if AddToInventoryOnCreation true, else spawn at base location
    /// </summary>
    public string SpawnLocationTemplateId { get; set; }
}
