/// <summary>
/// Represents a location-specific action that The Single Player can take.
/// Uses property-based matching to dynamically determine availability at different locations.
/// </summary>
public class LocationAction
{
/// <summary>
/// Unique identifier for this action
/// </summary>
public string Id { get; set; }

/// <summary>
/// Source location where this action was generated (if location-specific)
/// null = global action available at any matching location
/// non-null = only available at this specific location
/// </summary>
public string SourceLocationId { get; set; }

/// <summary>
/// Destination location for IntraVenueMove actions
/// Strongly-typed property replacing ID string parsing antipattern
/// Only populated for LocationActionType.IntraVenueMove
/// null for all other action types
/// </summary>
public string DestinationLocationId { get; set; }

/// <summary>
/// Display name shown to the player
/// </summary>
public string Name { get; set; }

/// <summary>
/// Detailed description of what this action does
/// </summary>
public string Description { get; set; }

/// <summary>
/// Required location properties for this action to be available.
/// The location must have ALL of these properties for the action to appear.
/// </summary>
public List<LocationPropertyType> RequiredProperties { get; set; } = new List<LocationPropertyType>();

/// <summary>
/// Optional location properties that enable this action.
/// The location must have AT LEAST ONE of these properties (if specified).
/// </summary>
public List<LocationPropertyType> OptionalProperties { get; set; } = new List<LocationPropertyType>();

/// <summary>
/// Properties that prevent this action from appearing.
/// If the location has ANY of these properties, the action is unavailable.
/// </summary>
public List<LocationPropertyType> ExcludedProperties { get; set; } = new List<LocationPropertyType>();

/// <summary>
/// Resource costs required to perform this action
/// </summary>
public ActionCosts Costs { get; set; } = new ActionCosts();

/// <summary>
/// Resources rewarded for performing this action
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
/// Obligation ID if this action launches an obligation (V2)
/// </summary>
public string ObligationId { get; set; }

/// <summary>
/// ChoiceTemplate source (Sir Brante layer - Scene-Situation architecture)
/// COMPOSITION not copy - access CompoundRequirement, ChoiceCost, ChoiceReward through this reference
///
/// null = Always-available action parsed directly from JSON (legacy pattern)
///        Uses direct Costs/Rewards properties above
///
/// non-null = Scene-spawned action generated from ChoiceTemplate at spawn time
///            ChoiceTemplate provides:
///            - RequirementFormula (CompoundRequirement with OR paths)
///            - CostTemplate (ChoiceCost with Coins/Resolve/TimeSegments)
///            - RewardTemplate (ChoiceReward with bonds/scales/states/scene spawns)
///
/// Enables unified action execution: All actions check ChoiceTemplate if present,
/// fall back to direct properties if null (legacy coexistence pattern)
/// </summary>
public ChoiceTemplate ChoiceTemplate { get; set; }

/// <summary>
/// THREE-TIER TIMING MODEL: Source Situation ID
/// Links ephemeral action to source Situation for cleanup after execution
/// Actions are QUERY-TIME instances (Tier 3), created when Situation activates
/// After action executes, GameFacade deletes ALL actions for this Situation
/// Next time player enters context, actions recreated fresh from ChoiceTemplates
/// </summary>
public string SituationId { get; set; }

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
/// Check if this action matches a given location's properties
/// </summary>
public bool MatchesLocation(Location location, TimeBlocks currentTime)
{
    if (location == null) return false;

    // Check location identity first (if action is location-specific)
    if (!string.IsNullOrEmpty(SourceLocationId) && location.Id != SourceLocationId)
        return false; // Location-specific action at wrong location

    // Get all active properties for the current time
    List<LocationPropertyType> activeProperties = location.GetActiveProperties(currentTime);

    // Check excluded properties first (fast rejection)
    if (ExcludedProperties.Any() && activeProperties.Any(p => ExcludedProperties.Contains(p)))
        return false;

    // Check required properties (all must be present)
    if (RequiredProperties.Any() && !RequiredProperties.All(p => activeProperties.Contains(p)))
        return false;

    // Check optional properties (at least one must be present if specified)
    if (OptionalProperties.Any() && !OptionalProperties.Any(p => activeProperties.Contains(p)))
        return false;

    return true;
}
}
