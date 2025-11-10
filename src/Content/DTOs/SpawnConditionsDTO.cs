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
/// Scene IDs that player must have completed
/// </summary>
public List<string> CompletedScenes { get; set; } = new List<string>();

/// <summary>
/// Choice IDs that player must have selected
/// </summary>
public List<string> ChoiceHistory { get; set; } = new List<string>();

/// <summary>
/// Minimum stat requirements
/// Key = ScaleType string, Value = minimum threshold
/// </summary>
public Dictionary<string, int> MinStats { get; set; } = new Dictionary<string, int>();

/// <summary>
/// Required inventory items
/// </summary>
public List<string> RequiredItems { get; set; } = new List<string>();

/// <summary>
/// Location visit count requirements
/// Key = LocationId, Value = minimum visit count
/// </summary>
public Dictionary<string, int> LocationVisits { get; set; } = new Dictionary<string, int>();
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
/// Entity state conditions DTO - relationships and reputation
/// </summary>
public class EntityStateConditionsDTO
{
/// <summary>
/// NPC bond requirements
/// Key = NpcId, Value = minimum bond strength
/// </summary>
public Dictionary<string, int> NPCBond { get; set; } = new Dictionary<string, int>();

/// <summary>
/// Location reputation requirements
/// Key = LocationId, Value = minimum reputation score
/// </summary>
public Dictionary<string, int> LocationReputation { get; set; } = new Dictionary<string, int>();

/// <summary>
/// Route travel count requirements
/// Key = RouteId, Value = minimum travel count
/// </summary>
public Dictionary<string, int> RouteTravelCount { get; set; } = new Dictionary<string, int>();

/// <summary>
/// Required entity properties
/// </summary>
public List<string> Properties { get; set; } = new List<string>();
}
