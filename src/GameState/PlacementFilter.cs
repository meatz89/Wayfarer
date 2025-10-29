/// <summary>
/// Categorical filter for selecting placement entities procedurally
/// NO concrete entity IDs - only categorical properties
/// Used by SceneTemplate to select NPCs/Locations/Routes at spawn time
/// Implements AI content generation pattern: JSON describes categories, spawner resolves to concrete entities
/// </summary>
public class PlacementFilter
{
    /// <summary>
    /// Type of placement: Location, NPC, or Route
    /// Determines which filter properties apply
    /// </summary>
    public PlacementType PlacementType { get; init; }

    // ==================== NPC FILTERS (when PlacementType == NPC) ====================

    /// <summary>
    /// Required personality types for NPC selection
    /// Example: ["Mercantile", "Commanding"] - NPC must have one of these personalities
    /// CATEGORICAL - no concrete NPC IDs
    /// </summary>
    public List<PersonalityType> PersonalityTypes { get; init; } = new List<PersonalityType>();

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
    /// Required location properties for categorical selection
    /// Example: [Secluded, Indoor, Private]
    /// Location must have ALL specified properties to match
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<LocationPropertyType> LocationProperties { get; init; } = new List<LocationPropertyType>();

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
