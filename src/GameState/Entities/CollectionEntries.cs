/// <summary>
/// Helper classes to replace Dictionary and HashSet with List-based implementations
/// following deterministic principles and avoiding non-deterministic data structures
/// </summary>

/// <summary>
/// Helper class for resource entries (replaces Dictionary<string, int>)
/// </summary>
public class ResourceEntry
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for NPC exchange card entries (replaces Dictionary<string, List<ExchangeCard>>)
/// HIGHLANDER: Object reference only, no string ID
/// </summary>
public class NPCExchangeCardEntry
{
    public NPC Npc { get; set; }
    public List<ExchangeCard> ExchangeCards { get; set; } = new List<ExchangeCard>();
}

/// <summary>
/// Helper class for skeleton registry entries (replaces Dictionary<string, string>)
/// </summary>
public class SkeletonRegistryEntry
{
    public string SkeletonKey { get; set; }
    public string ContentType { get; set; }
}

/// <summary>
/// Helper class for path card discovery entries (replaces Dictionary<string, bool>)
/// </summary>
public class PathCardDiscoveryEntry
{
    public string CardId { get; set; }
    public bool IsDiscovered { get; set; }
}

/// <summary>
/// Helper class for event deck position entries (replaces Dictionary<string, int>)
/// </summary>
public class EventDeckPositionEntry
{
    public string DeckId { get; set; }
    public int Position { get; set; }
}

/// <summary>
/// Helper class for path collection entries (replaces Dictionary<string, PathCardCollectionDTO>)
/// HIGHLANDER: Collection object contains Id - no need to store separately
/// </summary>
public class PathCollectionEntry
{
    // ADR-007: CollectionId DELETED - use Collection.Id instead (object has Id property)
    public PathCardCollectionDTO Collection { get; set; }
}

/// <summary>
/// Helper class for travel event entries (replaces Dictionary<string, TravelEventDTO>)
/// HIGHLANDER: TravelEvent object contains Id - no need to store separately
/// </summary>
public class TravelEventEntry
{
    // ADR-007: EventId DELETED - use TravelEvent.Id instead (object has Id property)
    public TravelEventDTO TravelEvent { get; set; }
}

/// <summary>
/// Helper class for stepped threshold entries (replaces Dictionary<int, float>)
/// Stores threshold levels with flat integer values
/// </summary>
public class SteppedThreshold
{
    public int Level { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Helper class for stat threshold entries (replaces Dictionary<PlayerStatType, int>)
/// Used in MentalCard, PhysicalCard for stat requirements
/// </summary>
public class StatThresholdEntry
{
    public PlayerStatType Stat { get; set; }
    public int Threshold { get; set; }
}

/// <summary>
/// Helper class for string-based stat requirement entries (replaces Dictionary<string, int>)
/// Used in PathCard, SceneApproach for stat requirements
/// </summary>
public class StatRequirementEntry
{
    public string StatName { get; set; }
    public int RequiredValue { get; set; }
}

/// <summary>
/// Helper class for token requirement entries (replaces Dictionary<string, int>)
/// Used in SocialCard for token requirements
/// </summary>
public class TokenRequirementEntry
{
    public string TokenType { get; set; }
    public int RequiredAmount { get; set; }
}

/// <summary>
/// Helper class for token gain entries (replaces Dictionary<string, int>)
/// Used in PathCard for token gains
/// </summary>
public class TokenGainEntry
{
    public string TokenType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for weather modification entries (replaces Dictionary<WeatherCondition, RouteModification>)
/// Used in RouteOption for weather-based route modifications
/// </summary>
public class WeatherModificationEntry
{
    public WeatherCondition Weather { get; set; }
    public RouteModification Modification { get; set; }
}

/// <summary>
/// Helper class for ConnectionType token entries (replaces Dictionary<ConnectionType, int>)
/// Used in Item, ExchangeSession, ResourceState for token tracking
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties, not generic key-value
/// </summary>
public class ConnectionTypeTokenEntry
{
    public ConnectionType Type { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for item count entries (replaces Dictionary<string, int>)
/// Used in SessionResourceSnapshot for tracking items by name
/// </summary>
public class ItemCountEntry
{
    public string ItemName { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Helper class for string token entries (replaces Dictionary<string, int>)
/// Used in SituationReward for token rewards
/// </summary>
public class StringTokenEntry
{
    public string TokenType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for location visit entries (replaces Dictionary<string, int>)
/// Used in SpawnConditions for visit count tracking
/// </summary>
public class LocationVisitEntry
{
    public string LocationId { get; set; }
    public int VisitCount { get; set; }
}

/// <summary>
/// Helper class for NPC bond entries (replaces Dictionary<string, int>)
/// Used in SpawnConditions for bond requirements
/// </summary>
public class NPCBondEntry
{
    public string NpcId { get; set; }
    public int BondStrength { get; set; }
}

/// <summary>
/// Helper class for location reputation entries (replaces Dictionary<string, int>)
/// Used in SpawnConditions for reputation requirements
/// </summary>
public class LocationReputationEntry
{
    public string LocationId { get; set; }
    public int ReputationScore { get; set; }
}

/// <summary>
/// Helper class for route travel count entries (replaces Dictionary<string, int>)
/// Used in SpawnConditions for travel count requirements
/// </summary>
public class RouteTravelCountEntry
{
    public string RouteId { get; set; }
    public int TravelCount { get; set; }
}

/// <summary>
/// Helper class for player stat entries (replaces Dictionary<PlayerStatType, int>)
/// Used in Context classes for player stat display
/// </summary>
public class PlayerStatEntry
{
    public PlayerStatType Stat { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Helper class for scale type entries (replaces Dictionary<ScaleType, int>)
/// Used in SpawnConditions for stat threshold requirements
/// </summary>
public class ScaleTypeEntry
{
    public ScaleType Scale { get; set; }
    public int Threshold { get; set; }
}

/// <summary>
/// Helper class for connection state modifier entries (replaces Dictionary<ConnectionState, int>)
/// Used in CardMechanics for state-based modifiers
/// </summary>
public class ConnectionStateModifierEntry
{
    public ConnectionState State { get; set; }
    public int Modifier { get; set; }
}

/// <summary>
/// Helper class for modifier parameter entries (replaces Dictionary<string, int>)
/// Used in PersonalityModifier for personality-specific parameters
/// </summary>
public class ModifierParameterEntry
{
    public string ParameterName { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Helper class for segment event draw entries (replaces Dictionary<string, string>)
/// Used in TravelSession for tracking which event was drawn for each segment
/// </summary>
public class SegmentEventDrawEntry
{
    public string SegmentId { get; set; }
    public string EventId { get; set; }
}

// ALL EXTENSION METHODS DELETED - Domain logic moved to Player.cs and GameWorld.cs
// Extension methods hide domain logic and violate architecture principles
// Use Player instance methods and GameWorld instance methods instead
