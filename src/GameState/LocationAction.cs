/// <summary>
/// Represents a location-specific action that The Single Player can take.
/// Uses property-based matching to dynamically determine availability at different locations.
/// </summary>
public class LocationAction
{
    /// <summary>
    /// Source location where this action was generated (if location-specific)
    /// HIGHLANDER: Object reference ONLY, no SourceLocationId
    /// null = global action available at any matching location
    /// non-null = only available at this specific location
    /// </summary>
    public Location SourceLocation { get; set; }

    /// <summary>
    /// Destination location for IntraVenueMove actions
    /// HIGHLANDER: Object reference ONLY, no DestinationLocationId
    /// Strongly-typed property replacing ID string parsing antipattern
    /// Only populated for LocationActionType.IntraVenueMove
    /// null for all other action types
    /// </summary>
    public Location DestinationLocation { get; set; }

    /// <summary>
    /// Display name shown to the player
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of what this action does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Required location capabilities for this action to be available.
    /// The location must have ALL of these capabilities for the action to appear (bitwise AND check).
    /// </summary>
    public LocationCapability RequiredCapabilities { get; set; } = LocationCapability.None;

    /// <summary>
    /// Optional location capabilities that enable this action.
    /// The location must have AT LEAST ONE of these capabilities (if specified).
    /// </summary>
    public LocationCapability OptionalCapabilities { get; set; } = LocationCapability.None;

    /// <summary>
    /// Capabilities that prevent this action from appearing.
    /// If the location has ANY of these capabilities, the action is unavailable.
    /// </summary>
    public LocationCapability ExcludedCapabilities { get; set; } = LocationCapability.None;

    /// <summary>
    /// Resource costs required to perform this action (ATMOSPHERIC PATTERN)
    /// Used when ChoiceTemplate is null (catalog-generated atmospheric actions)
    /// </summary>
    public ActionCosts Costs { get; set; } = new ActionCosts();

    /// <summary>
    /// Resources rewarded for performing this action (ATMOSPHERIC PATTERN)
    /// Used when ChoiceTemplate is null (catalog-generated atmospheric actions)
    /// </summary>
    public ActionRewards Rewards { get; set; } = new ActionRewards();

    /// <summary>
    /// Time required to complete this action in minutes
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Time blocks when this action is available (strongly-typed enum)
    /// </summary>
    public List<TimeBlocks> Availability { get; set; } = new List<TimeBlocks>();

    /// <summary>
    /// Priority for sorting when multiple actions match (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for execution dispatch - strongly typed enum validated by parser
    /// </summary>
    public LocationActionType ActionType { get; set; }

    /// <summary>
    /// Engagement type for tactical system integration (Mental, Physical, Social, Conversation)
    /// </summary>
    public string EngagementType { get; set; }

    /// <summary>
    /// Obligation if this action launches an obligation (V2)
    /// HIGHLANDER: Object reference ONLY, no ObligationId
    /// </summary>
    public Obligation Obligation { get; set; }

    /// <summary>
    /// ChoiceTemplate source (SCENE-BASED PATTERN - Scene-Situation architecture)
    /// COMPOSITION not copy - access CompoundRequirement, ChoiceCost, ChoiceReward through this reference
    ///
    /// PATTERN DISCRIMINATION:
    /// - IF ChoiceTemplate != null → SCENE-BASED action (use template costs/rewards)
    /// - IF ChoiceTemplate == null → ATMOSPHERIC action (use direct Costs/Rewards properties)
    ///
    /// Scene-spawned actions generated from ChoiceTemplate at spawn time.
    /// ChoiceTemplate provides:
    /// - RequirementFormula (CompoundRequirement with OR paths)
    /// - CostTemplate (ChoiceCost with Coins/Resolve/TimeSegments)
    /// - RewardTemplate (ChoiceReward with bonds/scales/states/scene spawns)
    ///
    /// See DUAL_TIER_ACTION_ARCHITECTURE.md for complete explanation.
    /// </summary>
    public ChoiceTemplate ChoiceTemplate { get; set; }

    /// <summary>
    /// THREE-TIER TIMING MODEL: Source Situation
    /// HIGHLANDER: Object reference ONLY, no SituationId
    /// Links ephemeral action to source Situation for cleanup after execution
    /// Actions are QUERY-TIME instances (Tier 3), created when Situation activates
    /// After action executes, GameFacade deletes ALL actions for this Situation
    /// Next time player enters context, actions recreated fresh from ChoiceTemplates
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// PERFECT INFORMATION: Scene spawn previews
    /// If this action spawns scenes (ChoiceTemplate.RewardTemplate.ScenesToSpawn),
    /// SceneFacade generates ScenePreview from SceneTemplate metadata
    /// Player sees WHERE scene will spawn, WHAT it contains, BEFORE selecting action
    /// Enables strategic decision-making with full knowledge of consequences
    ///
    /// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
    /// - OLD: Create Scene entity with State=Provisional, delete if not selected
    /// - NEW: Generate ScenePreview DTO from template, no entity until action executes
    /// </summary>
    public List<ScenePreview> ScenePreviews { get; set; } = new List<ScenePreview>();

    /// <summary>
    /// Check if this action matches a given location's capabilities
    /// </summary>
    public bool MatchesLocation(Location location, TimeBlocks currentTime)
    {
        if (location == null) return false;

        // Check location identity first (if action is location-specific)
        if (SourceLocation != null && location != SourceLocation)
            return false; // Location-specific action at wrong location

        // Check excluded capabilities first (fast rejection)
        if (ExcludedCapabilities != LocationCapability.None && (location.Capabilities & ExcludedCapabilities) != 0)
            return false; // Location has at least one excluded capability

        // Check required capabilities (all must be present)
        if (RequiredCapabilities != LocationCapability.None && (location.Capabilities & RequiredCapabilities) != RequiredCapabilities)
            return false; // Location missing at least one required capability

        // Check optional capabilities (at least one must be present if specified)
        if (OptionalCapabilities != LocationCapability.None && (location.Capabilities & OptionalCapabilities) == 0)
            return false; // Location has none of the optional capabilities

        return true;
    }
}
