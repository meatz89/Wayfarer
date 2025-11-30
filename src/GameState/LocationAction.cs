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

    // CAPABILITY PROPERTIES DELETED - capability matching replaced by orthogonal property checks
    // Actions are generated at parse-time with explicit SourceLocation binding
    // No runtime capability matching needed - action availability determined by generation logic

    /// <summary>
    /// HIGHLANDER: Unified costs and rewards (ATMOSPHERIC PATTERN)
    /// Consequence is the ONLY class for resource outcomes.
    /// Negative values = costs, Positive values = rewards
    /// Used when ChoiceTemplate is null (catalog-generated atmospheric actions)
    /// </summary>
    public Consequence Consequence { get; set; } = new Consequence();

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
    /// COMPOSITION not copy - access CompoundRequirement, OnSuccessConsequence, OnFailureConsequence through this reference
    ///
    /// PATTERN DISCRIMINATION:
    /// - IF ChoiceTemplate != null → SCENE-BASED action (use template consequences)
    /// - IF ChoiceTemplate == null → ATMOSPHERIC action (use direct Consequence property)
    ///
    /// HIGHLANDER: Both patterns use Consequence. The distinction is WHERE the Consequence comes from:
    /// - Atmospheric: Consequence created at parse-time by LocationActionCatalog
    /// - Scene-based: Consequence from ChoiceTemplate at query-time
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
    /// If this action spawns scenes (ChoiceTemplate.OnSuccessConsequence.ScenesToSpawn),
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
    /// Entity-derived scaled requirement for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// - Parse-time: Catalogue generates rhythm structure + tier-based values
    /// - Query-time: Entity-derived adjustments from RuntimeScalingContext
    ///
    /// Created by SceneFacade when building LocationAction from ChoiceTemplate.
    /// null = no scaling applied (use ChoiceTemplate.RequirementFormula directly)
    /// non-null = scaled version reflecting current entity context
    ///
    /// HIGHLANDER: Original RequirementFormula stays on ChoiceTemplate (immutable).
    /// This is a COPY with adjustments applied.
    /// </summary>
    public CompoundRequirement ScaledRequirement { get; set; }

    /// <summary>
    /// Entity-derived scaled consequence for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// Costs adjusted based on Location quality and NPC power dynamic.
    ///
    /// PATTERN: For scene-based actions (ChoiceTemplate != null):
    /// - ScaledConsequence = scaled version of ChoiceTemplate.Consequence
    /// - Use this for display, original for execution (scaling is UI hint)
    ///
    /// For atmospheric actions (ChoiceTemplate == null):
    /// - Consequence property is used directly (already scaled at parse-time)
    /// - ScaledConsequence is null
    /// </summary>
    public Consequence ScaledConsequence { get; set; }

    /// <summary>
    /// Check if this action is available at a given location.
    /// Actions are bound to SourceLocation at generation time - no capability matching needed.
    /// </summary>
    public bool MatchesLocation(Location location, TimeBlocks currentTime)
    {
        if (location == null) return false;

        // Check location identity (action is bound to specific location at generation time)
        if (SourceLocation != null && location != SourceLocation)
            return false;

        return true;
    }
}
