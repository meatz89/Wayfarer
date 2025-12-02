/// <summary>
/// Strongly-typed entry classes for List-based collections.
/// DOMAIN COLLECTION PRINCIPLE: Use List with explicit entry types, never key-value patterns.
/// </summary>

/// <summary>
/// Entry for resource tracking with explicit type and amount.
/// </summary>
public class ResourceEntry
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Entry for NPC exchange card mapping.
/// HIGHLANDER: Object reference only, no string ID.
/// </summary>
public class NPCExchangeCardEntry
{
    public NPC Npc { get; set; }
    public List<ExchangeCard> ExchangeCards { get; set; } = new List<ExchangeCard>();
}

/// <summary>
/// Entry for skeleton registry mapping.
/// </summary>
public class SkeletonRegistryEntry
{
    public string SkeletonKey { get; set; }
    public string ContentType { get; set; }
}

/// <summary>
/// Entry for path card discovery state.
/// </summary>
public class PathCardDiscoveryEntry
{
    public string CardId { get; set; }
    public bool IsDiscovered { get; set; }
}

/// <summary>
/// Entry for event deck position tracking.
/// </summary>
public class EventDeckPositionEntry
{
    public string DeckId { get; set; }
    public int Position { get; set; }
}

/// <summary>
/// Entry for path collection reference.
/// HIGHLANDER: Collection object contains Id - no need to store separately.
/// </summary>
public class PathCollectionEntry
{
    public PathCardCollectionDTO Collection { get; set; }
}

/// <summary>
/// Entry for travel event reference.
/// HIGHLANDER: TravelEvent object contains Id - no need to store separately.
/// </summary>
public class TravelEventEntry
{
    public TravelEventDTO TravelEvent { get; set; }
}

/// <summary>
/// Entry for stepped threshold levels with flat integer values.
/// </summary>
public class SteppedThreshold
{
    public int Level { get; set; }
    public int Value { get; set; }
}

// StatThresholdEntry DELETED - use explicit properties (InsightThreshold, RapportThreshold, etc.)

// StatRequirementEntry DELETED - use explicit properties (InsightRequirement, RapportRequirement, etc.)

// TokenRequirementEntry DELETED - DOMAIN COLLECTION PRINCIPLE
// SocialCard now uses explicit properties: TrustTokenRequirement, DiplomacyTokenRequirement, etc.

// TokenGainEntry DELETED - use explicit properties (TrustTokenGain, DiplomacyTokenGain, etc.)

// WeatherModificationEntry DELETED - use explicit properties (ClearWeatherModification, RainWeatherModification, etc.)

// ConnectionTypeTokenEntry DELETED - use explicit properties (TrustTokens, DiplomacyTokens, etc.)

/// <summary>
/// Entry for item count tracking.
/// Used in SessionResourceSnapshot for tracking items by name.
/// </summary>
public class ItemCountEntry
{
    public string ItemName { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Entry for string-based token tracking.
/// Used in SituationReward for token rewards.
/// </summary>
public class StringTokenEntry
{
    public string TokenType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Entry for location visit count tracking.
/// Used in SpawnConditions for visit count requirements.
/// </summary>
public class LocationVisitEntry
{
    public string LocationId { get; set; }
    public int VisitCount { get; set; }
}

/// <summary>
/// Entry for NPC bond strength tracking.
/// Used in SpawnConditions for bond requirements.
/// </summary>
public class NPCBondEntry
{
    public string NpcId { get; set; }
    public int BondStrength { get; set; }
}

/// <summary>
/// Entry for location reputation tracking.
/// Used in SpawnConditions for reputation requirements.
/// </summary>
public class LocationReputationEntry
{
    public string LocationId { get; set; }
    public int ReputationScore { get; set; }
}

/// <summary>
/// Entry for route travel count tracking.
/// Used in SpawnConditions for travel count requirements.
/// </summary>
public class RouteTravelCountEntry
{
    public string RouteId { get; set; }
    public int TravelCount { get; set; }
}

// PlayerStatEntry DELETED - use explicit properties (InsightLevel, RapportLevel, etc.)

// ScaleTypeEntry DELETED - use explicit properties (MinMorality, MinLawfulness, etc.)

// ConnectionStateModifierEntry DELETED - use explicit properties (DisconnectedModifier, GuardedModifier, etc.)

/// <summary>
/// Entry for modifier parameter values.
/// Used in PersonalityModifier for personality-specific parameters.
/// </summary>
public class ModifierParameterEntry
{
    public string ParameterName { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Entry for segment event draw tracking.
/// Used in TravelSession for tracking which event was drawn for each segment.
/// </summary>
public class SegmentEventDrawEntry
{
    public string SegmentId { get; set; }
    public string EventId { get; set; }
}

// TimeBlockProfessionsEntry, TimeBlockActionsEntry, TimeBlockDescriptionEntry DELETED
// Use explicit properties on Location instead: MorningProfessions, MiddayProfessions, etc.

// ALL EXTENSION METHODS DELETED - Domain logic moved to Player.cs and GameWorld.cs
