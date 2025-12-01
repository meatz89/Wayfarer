/// <summary>
/// DTO for PlacementFilter - categorical filters for procedural entity selection
/// Determines what NPCs/Locations/Routes match this Scene's spawning context
/// Maps to PlacementFilter domain entity
/// SIMPLICITY: Each property is SINGULAR (nullable string), not plural (List).
///   null = don't filter on this property (any value matches)
///   value = entity must have exactly this value
/// </summary>
public class PlacementFilterDTO
{
    /// <summary>
    /// Placement type classification
    /// Values: "Location", "NPC", "Route"
    /// </summary>
    public string PlacementType { get; set; }

    // ====================
    // NPC FILTERS (CATEGORICAL)
    // ====================

    /// <summary>
    /// Personality type to match
    /// Example: "Mercantile"
    /// Maps to PersonalityType enum value
    /// null = don't filter by personality
    /// </summary>
    public string PersonalityType { get; set; }

    /// <summary>
    /// Profession to match
    /// Example: "Merchant"
    /// Maps to Professions enum value
    /// null = don't filter by profession
    /// </summary>
    public string Profession { get; set; }

    /// <summary>
    /// Required relationship state
    /// Example: "Ally"
    /// Maps to NPCRelationship enum value
    /// null = don't filter by relationship
    /// </summary>
    public string RequiredRelationship { get; set; }

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
    /// Social standing for NPC selection
    /// Example: "Notable"
    /// Maps to NPCSocialStanding enum value
    /// null = don't filter by standing
    /// </summary>
    public string SocialStanding { get; set; }

    /// <summary>
    /// Story role for NPC selection
    /// Example: "Obstacle"
    /// Maps to NPCStoryRole enum value
    /// null = don't filter by role
    /// </summary>
    public string StoryRole { get; set; }

    /// <summary>
    /// Knowledge level for NPC selection
    /// Example: "Informed"
    /// Maps to NPCKnowledgeLevel enum value
    /// null = don't filter by knowledge
    /// </summary>
    public string KnowledgeLevel { get; set; }

    /// <summary>
    /// DEPRECATED: Use orthogonal categorical dimensions instead
    /// </summary>
    public List<string> NpcTags { get; set; } = new List<string>();

    // ====================
    // LOCATION FILTERS
    // ====================

    /// <summary>
    /// Location role to match (functional/narrative role)
    /// Example: "Hub", "Connective", "Rest"
    /// Maps to LocationRole enum value
    /// null = don't filter by role
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Player accessibility requirement
    /// true = only accessible locations
    /// false/null = any location
    /// </summary>
    public bool? IsPlayerAccessible { get; set; }

    /// <summary>
    /// Privacy level for location selection
    /// Example: "Private"
    /// Maps to LocationPrivacy enum value
    /// null = don't filter by privacy
    /// </summary>
    public string Privacy { get; set; }

    /// <summary>
    /// Safety level for location selection
    /// Example: "Safe"
    /// Maps to LocationSafety enum value
    /// null = don't filter by safety
    /// </summary>
    public string Safety { get; set; }

    /// <summary>
    /// Activity level for location selection
    /// Example: "Quiet"
    /// Maps to LocationActivity enum value
    /// null = don't filter by activity
    /// </summary>
    public string Activity { get; set; }

    /// <summary>
    /// Purpose for location selection
    /// Example: "Commerce"
    /// Maps to LocationPurpose enum value
    /// null = don't filter by purpose
    /// </summary>
    public string Purpose { get; set; }

    /// <summary>
    /// District ID filter (large categorical container)
    /// null = any district
    /// </summary>
    public string DistrictName { get; set; }

    /// <summary>
    /// Region ID filter (large categorical container)
    /// null = any region
    /// </summary>
    public string RegionName { get; set; }

    // ====================
    // ROUTE FILTERS
    // ====================

    /// <summary>
    /// Terrain type to match (orthogonal: natural geography)
    /// Example: "Forest", "Mountain", "Swamp"
    /// Maps to TerrainType enum value
    /// null = don't filter by terrain
    /// </summary>
    public string Terrain { get; set; }

    /// <summary>
    /// Structure type to match (orthogonal: built/constructed features)
    /// Example: "Bridge", "Road", "Ruin"
    /// Maps to StructureType enum value
    /// null = don't filter by structure
    /// </summary>
    public string Structure { get; set; }

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
    /// Categorical tags for route selection (still a list - route must have ALL tags)
    /// Example: ["Trade", "Dangerous", "Shortcut"]
    /// </summary>
    public List<string> RouteTags { get; set; } = new List<string>();

    /// <summary>
    /// Specific segment index for route situation placement
    /// Enables geographic specificity: situation activates at THIS segment, not entire route
    /// Example: SegmentIndex = 0 (first segment), 1 (second segment), 2 (third segment)
    /// null = situation spans entire route (any segment)
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
    // PLAYER STATE FILTERS (still lists - player can have multiple states)
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
