/// <summary>
/// Specification for generating a dependent location as part of self-contained scene
/// Scene defines these specs at authoring time, SceneInstantiator generates actual LocationDTOs at runtime
/// Generated locations flow through standard JSON → DTO → Domain pipeline
/// </summary>
public class DependentLocationSpec
{
    /// <summary>
    /// Template identifier for this location specification
    /// Situations bind to dependent locations via DependentLocationName property
    /// Becomes location Name after package generation
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Location name (generic, descriptive)
    /// Example: "Private Room", "Bath Chamber", "Training Grounds"
    /// When AI narrative added, it will regenerate with full context
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Location description (generic, descriptive)
    /// Example: "A private room for lodging."
    /// When AI narrative added, it will regenerate with full context
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// How to determine venue ID for generated location
    /// SameAsBase = Use base location's venue (most common for rooms in same building)
    /// </summary>
    public VenueIdSource VenueIdSource { get; set; } = VenueIdSource.SameAsBase;

    /// <summary>
    /// Strategy for placing location on hex grid
    /// Adjacent = One of 6 neighbors of base location
    /// SameVenue = No hex placement, intra-venue navigation
    /// </summary>
    public HexPlacementStrategy HexPlacement { get; set; } = HexPlacementStrategy.Adjacent;

    /// <summary>
    /// Location properties that define available actions
    /// Examples: "sleepingSpace", "restful", "indoor", "private"
    /// Used by LocationActionCatalog to generate available actions
    /// </summary>
    public List<string> Properties { get; set; } = new List<string>();

    /// <summary>
    /// Whether location starts in locked state
    /// Locked locations require key item to access
    /// Unlock action generated if IsLockedInitially=true and UnlockItemTemplateId set
    /// </summary>
    public bool IsLockedInitially { get; set; } = false;

    /// <summary>
    /// Template ID of item that unlocks this location
    /// If set, generates unlock requirement and action
    /// References DependentItemSpec by TemplateId
    /// </summary>
    public string UnlockItemTemplateId { get; set; }

    /// <summary>
    /// Whether location can be investigated for cubes
    /// Defaults to false for generated private locations
    /// </summary>
    public bool CanInvestigate { get; set; } = false;

    /// <summary>
    /// REQUIRED categorical dimension: Privacy level of location
    /// Valid values: "Public", "SemiPublic", "Private", "Restricted"
    /// FAIL-FAST: Must be explicitly set, no defaults
    /// </summary>
    public string Privacy { get; set; }

    /// <summary>
    /// REQUIRED categorical dimension: Safety level of location
    /// Valid values: "Dangerous", "Unsafe", "Neutral", "Safe", "Secure"
    /// FAIL-FAST: Must be explicitly set, no defaults
    /// </summary>
    public string Safety { get; set; }

    /// <summary>
    /// REQUIRED categorical dimension: Activity level of location
    /// Valid values: "Quiet", "Moderate", "Busy", "Crowded"
    /// FAIL-FAST: Must be explicitly set, no defaults
    /// </summary>
    public string Activity { get; set; }

    /// <summary>
    /// REQUIRED categorical dimension: Purpose of location
    /// Valid values: "Transit", "Dwelling", "Commerce", "Work", "Government", etc.
    /// FAIL-FAST: Must be explicitly set, no defaults
    /// Used by LocationPlacementService for venue matching
    /// </summary>
    public string Purpose { get; set; }
}

/// <summary>
/// How to determine venue ID for generated location
/// </summary>
public enum VenueIdSource
{
    /// <summary>
    /// Use the same venue as the base location
    /// Most common for rooms within the same building
    /// </summary>
    SameAsBase,

    /// <summary>
    /// Generate new venue (future extension)
    /// For creating entirely new buildings/areas
    /// </summary>
    GenerateNew
}
