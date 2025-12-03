/// <summary>
/// DTO for SpawnConditions - temporal eligibility criteria for scene spawning
/// Maps to SpawnConditions domain entity
/// Three evaluation dimensions: PlayerState, WorldState, EntityState
/// </summary>
public class SpawnConditionsDTO
{
    /// <summary>
    /// Player state conditions - progression and history requirements
    /// </summary>
    public PlayerStateConditionsDTO PlayerState { get; set; }

    /// <summary>
    /// World state conditions - temporal and environmental requirements
    /// </summary>
    public WorldStateConditionsDTO WorldState { get; set; }

    /// <summary>
    /// Entity state conditions - relationship and reputation requirements
    /// </summary>
    public EntityStateConditionsDTO EntityState { get; set; }

    /// <summary>
    /// Combination logic for multi-dimension conditions
    /// "AND" = ALL dimensions must pass
    /// "OR" = ANY dimension passing allows spawn
    /// </summary>
    public string CombinationLogic { get; set; }
}

/// <summary>
/// Player state conditions DTO - progression and history
/// </summary>
public class PlayerStateConditionsDTO
{
    /// <summary>
    /// Minimum stat requirements
    /// DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    /// </summary>
    public List<StatThresholdDTO> MinStats { get; set; } = new List<StatThresholdDTO>();

    /// <summary>
    /// Required inventory items
    /// </summary>
    public List<string> RequiredItems { get; set; } = new List<string>();

    // LocationVisits DELETED - §8.30: SpawnConditions must reference existing entities via object refs
}

/// <summary>
/// World state conditions DTO - temporal and environmental
/// </summary>
public class WorldStateConditionsDTO
{
    /// <summary>
    /// Required weather state (string for parse-time validation)
    /// </summary>
    public string Weather { get; set; }

    /// <summary>
    /// Required time block (string for parse-time validation)
    /// </summary>
    public string TimeBlock { get; set; }

    /// <summary>
    /// Minimum day requirement
    /// </summary>
    public int? MinDay { get; set; }

    /// <summary>
    /// Maximum day requirement
    /// </summary>
    public int? MaxDay { get; set; }

    /// <summary>
    /// Required location states (strings for parse-time validation)
    /// </summary>
    public List<string> LocationStates { get; set; } = new List<string>();
}

/// <summary>
/// Entity state conditions DTO - entity properties
/// §8.30: NPCBond, LocationReputation, RouteTravelCount DELETED - must use object refs not string IDs
/// </summary>
public class EntityStateConditionsDTO
{
    /// <summary>
    /// Required entity properties
    /// </summary>
    public List<string> Properties { get; set; } = new List<string>();
}
