/// <summary>
/// DTO for PlacementFilter - categorical filters for procedural entity selection
/// Determines what NPCs/Locations/Routes match this Scene's spawning context
/// Maps to PlacementFilter domain entity
/// </summary>
public class PlacementFilterDTO
{
    /// <summary>
    /// Placement type classification
    /// Values: "Location", "NPC", "Route"
    /// </summary>
    public string PlacementType { get; set; }

    // ====================
    // CONCRETE ID BINDINGS (ALTERNATIVE TO CATEGORICAL FILTERS)
    // ====================

    // NpcId/LocationId/RouteId DELETED - 100% categorical resolution
    // NEW ARCHITECTURE: All placement uses categorical filters, no concrete IDs
    // EntityResolver finds/creates entities matching categories at spawn time

    // ====================
    // NPC FILTERS (CATEGORICAL)
    // ====================

    /// <summary>
    /// Personality types to match
    /// Example: ["Innocent", "Cunning", "Authoritative"]
    /// Maps to PersonalityType enum values
    /// </summary>
    public List<string> PersonalityTypes { get; set; } = new List<string>();

    /// <summary>
    /// Professions to match
    /// Example: ["Merchant", "Scholar", "Guard"]
    /// Maps to Professions enum values
    /// </summary>
    public List<string> Professions { get; set; } = new List<string>();

    /// <summary>
    /// Required relationship states
    /// Example: ["Ally", "Rival", "Neutral"]
    /// Maps to NPCRelationship enum values
    /// </summary>
    public List<string> RequiredRelationships { get; set; } = new List<string>();

    /// <summary>
    /// Minimum NPC tier requirement
    /// null = no minimum
    /// </summary>
    public int? MinTier { get; set; }

    /// <summary>
    /// Maximum NPC tier requirement
    /// null = no maximum
    /// </summary>
    public int? MaxTier { get; set; }

    /// <summary>
    /// Minimum bond strength required
    /// null = no minimum
    /// </summary>
    public int? MinBond { get; set; }

    /// <summary>
    /// Maximum bond strength allowed
    /// null = no maximum
    /// </summary>
    public int? MaxBond { get; set; }

    /// <summary>
    /// DEPRECATED: Use orthogonal categorical dimensions instead
    /// </summary>
    public List<string> NpcTags { get; set; } = new List<string>();

    // Orthogonal Categorical Dimensions - NPC
    // String values from JSON parsed to enums by SceneTemplateParser

    /// <summary>
    /// Social standing dimension for NPC selection
    /// Example: ["Notable", "Authority"]
    /// Maps to NPCSocialStanding enum values
    /// </summary>
    public List<string> SocialStandings { get; set; } = new List<string>();

    /// <summary>
    /// Story role dimension for NPC selection
    /// Example: ["Obstacle", "Facilitator"]
    /// Maps to NPCStoryRole enum values
    /// </summary>
    public List<string> StoryRoles { get; set; } = new List<string>();

    /// <summary>
    /// Knowledge level dimension for NPC selection
    /// Example: ["Informed", "Expert"]
    /// Maps to NPCKnowledgeLevel enum values
    /// </summary>
    public List<string> KnowledgeLevels { get; set; } = new List<string>();

    // ====================
    // LOCATION FILTERS
    // ====================

    /// <summary>
    /// Location types to match
    /// Example: ["Inn", "Tavern", "Market"]
    /// Maps to LocationTypes enum values
    /// </summary>
    public List<string> LocationTypes { get; set; } = new List<string>();

    /// <summary>
    /// Location capabilities to match
    /// Example: ["Crossroads", "Commercial", "Indoor"]
    /// Maps to LocationCapability Flags enum values
    /// </summary>
    public List<string> Capabilities { get; set; } = new List<string>();

    /// <summary>
    /// Player accessibility requirement
    /// true = only accessible locations
    /// false/null = any location
    /// </summary>
    public bool? IsPlayerAccessible { get; set; }

    /// <summary>
    /// DEPRECATED: Use orthogonal categorical dimensions instead (kept only for DEPENDENT_LOCATION markers)
    /// </summary>
    public List<string> LocationTags { get; set; } = new List<string>();

    // Orthogonal Categorical Dimensions - Location
    // String values from JSON parsed to enums by SceneTemplateParser

    /// <summary>
    /// Privacy dimension for location selection
    /// Example: ["SemiPublic", "Private"]
    /// Maps to LocationPrivacy enum values
    /// </summary>
    public List<string> PrivacyLevels { get; set; } = new List<string>();

    /// <summary>
    /// Safety dimension for location selection
    /// Example: ["Safe"]
    /// Maps to LocationSafety enum values
    /// </summary>
    public List<string> SafetyLevels { get; set; } = new List<string>();

    /// <summary>
    /// Activity dimension for location selection
    /// Example: ["Quiet", "Moderate"]
    /// Maps to LocationActivity enum values
    /// </summary>
    public List<string> ActivityLevels { get; set; } = new List<string>();

    /// <summary>
    /// Purpose dimension for location selection
    /// Example: ["Dwelling", "Commerce"]
    /// Maps to LocationPurpose enum values
    /// </summary>
    public List<string> Purposes { get; set; } = new List<string>();

    /// <summary>
    /// District ID filter (large categorical container)
    /// null = any district
    /// </summary>
    public string DistrictId { get; set; }

    /// <summary>
    /// Region ID filter (large categorical container)
    /// null = any region
    /// </summary>
    public string RegionId { get; set; }

    // ====================
    // ROUTE FILTERS
    // ====================

    /// <summary>
    /// Terrain types to match
    /// Example: ["Forest", "Mountain", "Road"]
    /// </summary>
    public List<string> TerrainTypes { get; set; } = new List<string>();

    /// <summary>
    /// Route difficulty tier filter
    /// null = any tier
    /// </summary>
    public int? RouteTier { get; set; }

    /// <summary>
    /// Minimum difficulty rating (0-100 scale)
    /// null = no minimum
    /// </summary>
    public int? MinDifficulty { get; set; }

    /// <summary>
    /// Maximum difficulty rating (0-100 scale)
    /// null = no maximum
    /// </summary>
    public int? MaxDifficulty { get; set; }

    /// <summary>
    /// Categorical tags for route selection
    /// Example: ["Trade", "Dangerous", "Shortcut"]
    /// </summary>
    public List<string> RouteTags { get; set; } = new List<string>();

    /// <summary>
    /// Specific segment index for route situation placement
    /// Enables geographic specificity: situation activates at THIS segment, not entire route
    /// Example: SegmentIndex = 0 (first segment), 1 (second segment), 2 (third segment)
    /// null = situation spans entire route (any segment)
    /// ARCHITECTURAL FOUNDATION: Required for route segment situations (Tutorial A3 pattern)
    /// VERISIMILITUDE: "Fallen tree at segment 1" vs abstract "tree somewhere on route"
    /// </summary>
    public int SegmentIndex { get; set; }

    // ====================
    // SYSTEM CONTROL
    // ====================

    /// <summary>
    /// Selection strategy when multiple entities match
    /// Values: "Random", "First", "Closest", "HighestBond", "LeastRecent"
    /// Maps to PlacementSelectionStrategy enum
    /// </summary>
    public string SelectionStrategy { get; set; }

    /// <summary>
    /// Exclude recently used entities from selection
    /// Improves variety
    /// </summary>
    public bool ExcludeRecentlyUsed { get; set; }

    // ====================
    // PLAYER STATE FILTERS
    // ====================

    /// <summary>
    /// Required active states for spawn
    /// Example: ["Injured", "Suspicious"]
    /// Maps to StateType enum values
    /// </summary>
    public List<string> RequiredStates { get; set; } = new List<string>();

    /// <summary>
    /// Forbidden active states (cannot spawn if player has these)
    /// Example: ["Wounded", "Exhausted"]
    /// Maps to StateType enum values
    /// </summary>
    public List<string> ForbiddenStates { get; set; } = new List<string>();

    /// <summary>
    /// Required achievement IDs
    /// </summary>
    public List<string> RequiredAchievements { get; set; } = new List<string>();

    /// <summary>
    /// Scale requirements (behavioral spectrum gates)
    /// Example: Morality scale must be between -5 and +5
    /// </summary>
    public List<ScaleRequirementDTO> ScaleRequirements { get; set; } = new List<ScaleRequirementDTO>();
}

/// <summary>
/// DTO for Scale Requirement within PlacementFilter
/// Constrains Scene spawning based on player behavioral spectrum position
/// </summary>
public class ScaleRequirementDTO
{
    /// <summary>
    /// Which scale to check
    /// Values: "Morality", "Lawfulness", "Method", "Caution", "Transparency", "Fame"
    /// </summary>
    public string ScaleType { get; set; }

    /// <summary>
    /// Minimum scale value (inclusive)
    /// null = no minimum
    /// </summary>
    public int? MinValue { get; set; }

    /// <summary>
    /// Maximum scale value (inclusive)
    /// null = no maximum
    /// </summary>
    public int? MaxValue { get; set; }
}
