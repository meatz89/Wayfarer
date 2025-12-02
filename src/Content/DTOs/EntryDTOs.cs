/// <summary>
/// Entry DTO classes to replace Dictionary patterns in DTOs
/// Following DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
/// </summary>

/// <summary>
/// DTO for stat requirements (replaces Dictionary<string, int>)
/// Used in PathCardDTO, SceneApproachDTO for stat requirements
/// </summary>
public class StatRequirementDTO
{
    public string Stat { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// DTO for stat thresholds (replaces Dictionary<string, int>)
/// Used in MentalCardDTO, PhysicalCardDTO, SpawnConditionsDTO for stat thresholds
/// </summary>
public class StatThresholdDTO
{
    public string Stat { get; set; }
    public int Threshold { get; set; }
}

/// <summary>
/// DTO for token entries (replaces Dictionary<string, int>)
/// Used in PathCardDTO, ItemDTO, NPCDTO, StrangerRewardDTO, ExchangeDTO for token properties
/// </summary>
public class TokenEntryDTO
{
    public string TokenType { get; set; }
    public int Amount { get; set; }
}


/// <summary>
/// Entry for stat XP rewards
/// Used in: ObligationDTO.CompletionRewardXP
/// </summary>
public class StatXPEntry
{
    public string StatType { get; set; }
    public int XPAmount { get; set; }
}

/// <summary>
/// Entry for discovery quantity requirements
/// Used in: ObligationDTO.PhaseRequirements.DiscoveryQuantities
/// </summary>
public class DiscoveryQuantityEntry
{
    public string DiscoveryType { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Entry for achievement grant conditions
/// Used in: AchievementDTO.GrantConditions
/// </summary>
public class AchievementConditionEntry
{
    public string ConditionType { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Entry for card counts in deck definitions
/// Used in: CardDeckDTO.CardCounts
/// </summary>
public class CardCountEntry
{
    public string CardId { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Entry for NPC relationship deltas
/// Used in: EmergencySituationDTO.EmergencyOutcome.NPCRelationshipDeltas
/// </summary>
public class NPCRelationshipDeltaEntry
{
    public string NpcId { get; set; }
    public int Delta { get; set; }
}

/// <summary>
/// Entry for available professions by time block
/// Used in: LocationDTO.AvailableProfessionsByTime
/// </summary>
public class ProfessionsByTimeEntry
{
    public string TimeBlock { get; set; }
    public List<string> Professions { get; set; } = new List<string>();
}

/// <summary>
/// Entry for route discovery contexts by NPC
/// Used in: RouteDiscoveryDTO.DiscoveryContexts
/// </summary>
public class RouteDiscoveryContextEntry
{
    public string NpcId { get; set; }
    public RouteDiscoveryContextDTO Context { get; set; }
}
