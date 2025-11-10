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
// NPC FILTERS
// ====================

/// <summary>
/// Concrete NPC ID binding (tutorial/explicit binding pattern)
/// When specified, bypasses categorical filtering and binds to specific NPC
/// Example: "elena" binds tutorial to Elena regardless of her categorical properties
/// null = use categorical filtering (personalityTypes, minBond, etc.)
/// </summary>
public string NpcId { get; set; }

/// <summary>
/// Personality types to match
/// Example: ["Innocent", "Cunning", "Authoritative"]
/// Maps to PersonalityType enum values
/// </summary>
public List<string> PersonalityTypes { get; set; } = new List<string>();

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
/// Categorical NPC tags
/// Example: ["merchant", "guard", "scholar"]
/// </summary>
public List<string> NpcTags { get; set; } = new List<string>();

// ====================
// LOCATION FILTERS
// ====================

/// <summary>
/// Concrete Location ID binding (tutorial/explicit binding pattern)
/// When specified, bypasses categorical filtering and binds to specific Location
/// Example: "fountain_plaza" binds scene to Town Square fountain
/// null = use categorical filtering (locationProperties, locationTags, etc.)
/// </summary>
public string LocationId { get; set; }

/// <summary>
/// Location properties to match
/// Example: ["Urban", "Dangerous", "Secluded"]
/// Maps to LocationPropertyType enum values
/// </summary>
public List<string> LocationProperties { get; set; } = new List<string>();

/// <summary>
/// Categorical location tags
/// Example: ["tavern", "workshop", "warehouse"]
/// </summary>
public List<string> LocationTags { get; set; } = new List<string>();

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
/// Minimum danger rating (0-100 scale)
/// null = no minimum
/// </summary>
public int? MinDangerRating { get; set; }

/// <summary>
/// Maximum danger rating (0-100 scale)
/// null = no maximum
/// </summary>
public int? MaxDangerRating { get; set; }

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
