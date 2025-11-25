/// <summary>
/// DTO for DependentLocationSpec - declarative specification for locations that scenes create dynamically
/// Self-contained pattern: Scene generates JSON package with these specs → PackageLoader → Standard parsing
/// Maps to DependentLocationSpec domain entity
/// </summary>
public class DependentLocationSpecDTO
{
    /// <summary>
    /// Template identifier for this location specification
    /// Used to construct location Name during package generation
    /// Example: "private_room"
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Generic, descriptive name for generated location
    /// Used as-is without AI generation until narrative system implemented
    /// Example: "Private Room", "Bath Chamber", "Training Grounds"
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Generic, descriptive description for generated location
    /// Used as-is without AI generation until narrative system implemented
    /// Example: "A private room for lodging.", "A private bathing chamber."
    /// </summary>
    public string Description { get; set; }

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

    /// <summary>
    /// REQUIRED: Privacy level categorical dimension
    /// Valid values: "Public", "SemiPublic", "Private", "Restricted"
    /// FAIL-FAST: Must be explicitly set in JSON, no defaults
    /// </summary>
    public string Privacy { get; set; }

    /// <summary>
    /// REQUIRED: Safety level categorical dimension
    /// Valid values: "Dangerous", "Unsafe", "Neutral", "Safe", "Secure"
    /// FAIL-FAST: Must be explicitly set in JSON, no defaults
    /// </summary>
    public string Safety { get; set; }

    /// <summary>
    /// REQUIRED: Activity level categorical dimension
    /// Valid values: "Quiet", "Moderate", "Busy", "Crowded"
    /// FAIL-FAST: Must be explicitly set in JSON, no defaults
    /// </summary>
    public string Activity { get; set; }

    /// <summary>
    /// REQUIRED: Purpose categorical dimension
    /// Valid values: "Transit", "Dwelling", "Commerce", "Work", "Government", "Education", etc.
    /// FAIL-FAST: Must be explicitly set in JSON, no defaults
    /// Used by LocationPlacementService for venue matching
    /// </summary>
    public string Purpose { get; set; }
}
