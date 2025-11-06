/// <summary>
/// DTO for DependentLocationSpec - declarative specification for locations that scenes create dynamically
/// Self-contained pattern: Scene generates JSON package with these specs → PackageLoader → Standard parsing
/// Maps to DependentLocationSpec domain entity
/// </summary>
public class DependentLocationSpecDTO
{
    /// <summary>
    /// Template identifier used in marker references
    /// Example: "private_room" becomes marker "generated:private_room"
    /// Also used to construct actual ID: "{sceneId}_{templateId}"
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Name pattern with placeholders replaced by PlaceholderReplacer at finalization
    /// Supported: {NPCName}, {LocationName}, {PlayerName}, {RouteName}
    /// Example: "{NPCName}'s Private Room"
    /// </summary>
    public string NamePattern { get; set; }

    /// <summary>
    /// Description pattern with placeholders replaced by PlaceholderReplacer at finalization
    /// Supported: {NPCName}, {LocationName}, {PlayerName}, {RouteName}
    /// Example: "A private room where {NPCName} provides lodging services."
    /// </summary>
    public string DescriptionPattern { get; set; }

    /// <summary>
    /// Venue ID determination strategy
    /// "SameAsBase": Use base location's venue (typical for rooms within inn)
    /// "GenerateNew": Create new venue (for separate buildings)
    /// Defaults to "SameAsBase" if not specified
    /// </summary>
    public string VenueIdSource { get; set; }

    /// <summary>
    /// Hex placement strategy relative to base location
    /// "Adjacent": One of 6 neighboring hexes
    /// "SameVenue": No hex needed (intra-venue instant travel)
    /// "Distance": Specific distance away (not yet implemented)
    /// "Random": Random within radius (not yet implemented)
    /// Defaults to "Adjacent" if not specified
    /// </summary>
    public string HexPlacement { get; set; }

    /// <summary>
    /// Location properties (semantic tags)
    /// Example: ["sleepingSpace", "restful", "indoor", "private"]
    /// Maps to LocationProperty enum values
    /// </summary>
    public List<string> Properties { get; set; }

    /// <summary>
    /// Whether location starts locked
    /// true: Requires unlock action/item to access
    /// false: Accessible immediately
    /// Defaults to false if not specified
    /// </summary>
    public bool IsLockedInitially { get; set; } = false;

    /// <summary>
    /// Template ID of item that unlocks this location
    /// References another DependentItemSpec by TemplateId
    /// Becomes marker reference: "generated:{UnlockItemTemplateId}"
    /// null = no item required (unlocked through choice reward)
    /// </summary>
    public string UnlockItemTemplateId { get; set; }

    /// <summary>
    /// Whether location can be investigated
    /// true: Player can search for clues/items
    /// false: No investigation actions available
    /// Defaults to false if not specified
    /// </summary>
    public bool CanInvestigate { get; set; } = false;
}
