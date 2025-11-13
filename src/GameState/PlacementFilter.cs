/// <summary>
/// Filter for selecting placement entities at spawn time via CATEGORICAL PROPERTIES ONLY
/// Implements AI content generation pattern: JSON describes categories (PersonalityTypes, LocationProperties),
/// spawner queries GameWorld entities and applies SelectionStrategy to choose from matches
/// Tutorial pattern uses SceneSpawnReward.SpecificPlacementId for concrete binding (NOT this filter)
/// HIGHLANDER: One pattern only - categorical queries, no concrete IDs
/// </summary>
public class PlacementFilter
{
    /// <summary>
    /// Type of placement: Location, NPC, or Route
    /// Determines which filter properties apply
    /// </summary>
    public PlacementType PlacementType { get; init; }

    // ==================== CATEGORICAL SEARCH ====================

    /// <summary>
    /// Strategy for selecting ONE entity when multiple candidates match the filter
    /// Defaults to Random (uniform distribution)
    /// </summary>
    public PlacementSelectionStrategy SelectionStrategy { get; init; } = PlacementSelectionStrategy.Random;

    // ==================== NPC FILTERS (when PlacementType == NPC) ====================

    /// <summary>
    /// Required personality types for NPC selection
    /// Example: ["Mercantile", "Commanding"] - NPC must have one of these personalities
    /// CATEGORICAL - no concrete NPC IDs
    /// </summary>
    public List<PersonalityType> PersonalityTypes { get; init; } = new List<PersonalityType>();

    /// <summary>
    /// Required professions for NPC selection
    /// Example: [Merchant, Scholar, Guard]
    /// NPC must have one of these professions to match
    /// </summary>
    public List<Professions> Professions { get; init; } = new List<Professions>();

    /// <summary>
    /// Required relationship states for NPC selection
    /// Example: [Ally, Rival, Neutral]
    /// NPC must have one of these relationship states with player
    /// </summary>
    public List<NPCRelationship> RequiredRelationships { get; init; } = new List<NPCRelationship>();

    /// <summary>
    /// Minimum NPC tier requirement
    /// null = no minimum
    /// Example: MinTier = 2 means "only select tier 2+ NPCs"
    /// </summary>
    public int? MinTier { get; init; }

    /// <summary>
    /// Maximum NPC tier requirement
    /// null = no maximum
    /// Example: MaxTier = 3 means "only select tier 1-3 NPCs"
    /// </summary>
    public int? MaxTier { get; init; }

    /// <summary>
    /// Minimum bond strength required with player
    /// Example: MinBond = 10 means "select NPCs player has bond 10+"
    /// null = no minimum requirement
    /// </summary>
    public int? MinBond { get; init; }

    /// <summary>
    /// Maximum bond strength allowed with player
    /// Example: MaxBond = 5 means "select NPCs player has bond 5 or less"
    /// null = no maximum requirement
    /// Enables "stranger" or "unfamiliar" categorical selection
    /// </summary>
    public int? MaxBond { get; init; }

    /// <summary>
    /// Categorical tags for NPC selection
    /// Example: ["Wealthy", "UrbanResident", "Authority"]
    /// NPC must have ALL specified tags to match
    /// </summary>
    public List<string> NpcTags { get; init; } = new List<string>();

    // ==================== LOCATION FILTERS (when PlacementType == Location) ====================

    /// <summary>
    /// Required location types for categorical selection
    /// Example: [Inn, Tavern, Market]
    /// Location must match one of these types
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<LocationTypes> LocationTypes { get; init; } = new List<LocationTypes>();

    /// <summary>
    /// Required location properties for categorical selection
    /// Example: [Secluded, Indoor, Private]
    /// Location must have ALL specified properties to match
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<LocationPropertyType> LocationProperties { get; init; } = new List<LocationPropertyType>();

    /// <summary>
    /// Player accessibility requirement
    /// true = only accessible locations (visited OR unlocked)
    /// false/null = any location regardless of accessibility
    /// </summary>
    public bool? IsPlayerAccessible { get; init; }

    /// <summary>
    /// Categorical tags for location selection
    /// Example: ["Marketplace", "Noble", "Industrial"]
    /// Location must have ALL specified tags to match
    /// </summary>
    public List<string> LocationTags { get; init; } = new List<string>();

    /// <summary>
    /// District identifier filter
    /// null = any district
    /// Specified = location must be in this district
    /// NOTE: This is concrete ID, but districts are large categorical containers
    /// </summary>
    public string DistrictId { get; init; }

    /// <summary>
    /// Region identifier filter
    /// null = any region
    /// Specified = location must be in this region
    /// NOTE: This is concrete ID, but regions are large categorical containers
    /// </summary>
    public string RegionId { get; init; }

    // ==================== ROUTE FILTERS (when PlacementType == Route) ====================

    /// <summary>
    /// Terrain types for route selection
    /// Example: [Urban, Forest, Mountain]
    /// Route must match one of these terrain types
    /// </summary>
    public List<string> TerrainTypes { get; init; } = new List<string>();

    /// <summary>
    /// Route difficulty tier requirement
    /// null = any difficulty
    /// Specified = route must match this tier
    /// </summary>
    public int? RouteTier { get; init; }

    /// <summary>
    /// Minimum difficulty rating for route selection (uses DangerRating property)
    /// null = no minimum
    /// Used to filter scenes for dangerous routes
    /// Example: MinDifficulty = 30 means "only spawn on routes with danger 30+"
    /// </summary>
    public int? MinDifficulty { get; init; }

    /// <summary>
    /// Maximum difficulty rating for route selection (uses DangerRating property)
    /// null = no maximum
    /// Used to filter scenes for safer routes
    /// Example: MaxDifficulty = 50 means "only spawn on routes with danger <= 50"
    /// </summary>
    public int? MaxDifficulty { get; init; }

    /// <summary>
    /// Categorical tags for route selection
    /// Example: ["Trade", "Dangerous", "Shortcut"]
    /// Route must have ALL specified tags to match
    /// </summary>
    public List<string> RouteTags { get; init; } = new List<string>();

    // ==================== VARIETY CONTROL ====================

    /// <summary>
    /// Exclude recently used entities from selection
    /// Improves variety by preventing same location/NPC/route appearing repeatedly
    /// Uses LastUsedDay property to filter out recent selections
    /// </summary>
    public bool ExcludeRecentlyUsed { get; init; }

    // ==================== PLAYER STATE FILTERS (applies to all placement types) ====================

    /// <summary>
    /// Player must have these states for Scene to spawn
    /// Example: [Injured, Exhausted, Wanted]
    /// Player must have ALL specified states
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<StateType> RequiredStates { get; init; } = new List<StateType>();

    /// <summary>
    /// Player must NOT have these states for Scene to spawn
    /// Example: [Rested, Healthy]
    /// Player must have NONE of these states
    /// Enables "only when wounded" or "only when fresh" scenarios
    /// </summary>
    public List<StateType> ForbiddenStates { get; init; } = new List<StateType>();

    /// <summary>
    /// Player must have these achievements for Scene to spawn
    /// Example: ["FirstCombatVictory", "MercantileGuildMember"]
    /// Player must have ALL specified achievements
    /// Achievement IDs are concrete, but they represent categorical milestones
    /// </summary>
    public List<string> RequiredAchievements { get; init; } = new List<string>();

    /// <summary>
    /// Player scale requirements for spawning
    /// Example: Morality >= +5, Reputation >= -3
    /// All requirements must be met
    /// </summary>
    public List<ScaleRequirement> ScaleRequirements { get; init; } = new List<ScaleRequirement>();
}

/// <summary>
/// Scale requirement for player behavioral spectrum filtering
/// Example: "Morality must be at least +5" or "Reputation must be at most -3"
/// </summary>
public class ScaleRequirement
{
    /// <summary>
    /// Which scale to check
    /// </summary>
    public ScaleType ScaleType { get; init; }

    /// <summary>
    /// Minimum required value (inclusive)
    /// null = no minimum
    /// Example: MinValue = 5 means "scale must be >= 5"
    /// </summary>
    public int? MinValue { get; init; }

    /// <summary>
    /// Maximum allowed value (inclusive)
    /// null = no maximum
    /// Example: MaxValue = -3 means "scale must be <= -3"
    /// </summary>
    public int? MaxValue { get; init; }
}
