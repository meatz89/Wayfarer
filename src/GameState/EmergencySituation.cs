/// <summary>
/// Immutable template defining an emergency situation archetype.
/// Templates are loaded from JSON and never mutated at runtime.
/// HIGHLANDER: Template IDs are allowed (immutable archetypes).
/// </summary>
public class EmergencySituation
{
    // ADR-007: Id property - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Trigger conditions
    public int? TriggerDay { get; set; }
    public int? TriggerSegment { get; set; }
    // HIGHLANDER: Object references ONLY, no TriggerLocationIds
    public List<Location> TriggerLocations { get; set; } = new List<Location>();
    public int ResponseWindowSegments { get; set; }  // How long player has to respond

    // Response options
    public List<EmergencyResponse> Responses { get; set; } = new List<EmergencyResponse>();

    // Outcome if ignored
    public EmergencyOutcome IgnoreOutcome { get; set; }
}

/// <summary>
/// Mutable runtime state for an active emergency.
/// HIGHLANDER: NO Id property - identified by object reference only.
/// References immutable EmergencySituation template.
/// </summary>
public class ActiveEmergencyState
{
    // HIGHLANDER: Object reference to template, not template ID
    public EmergencySituation Template { get; set; }

    // Mutable state
    public bool IsTriggered { get; set; }
    public bool IsResolved { get; set; }
    public int? TriggeredAtSegment { get; set; }
}

/// <summary>
/// A response option for an emergency situation.
/// Costs resources immediately, grants outcomes, spawns follow-up content.
/// </summary>
public class EmergencyResponse
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string ResponseText { get; set; }
    public string Description { get; set; }

    // Costs
    public int StaminaCost { get; set; }
    public int HealthCost { get; set; }
    public int CoinCost { get; set; }
    public int TimeCost { get; set; }  // Segments

    // Outcomes
    public EmergencyOutcome Outcome { get; set; }
}

/// <summary>
/// Consequences of responding to (or ignoring) an emergency.
/// </summary>
public class EmergencyOutcome
{
    public int RelationshipDelta { get; set; }  // General reputation change
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<NPCRelationshipDelta> NPCRelationshipDeltas { get; set; } = new List<NPCRelationshipDelta>();
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    // HIGHLANDER: Object references ONLY, no SpawnedSituationIds
    public List<Situation> SpawnedSituations { get; set; } = new List<Situation>();
    // HIGHLANDER: Object references ONLY, no GrantedItemIds
    public List<Item> GrantedItems { get; set; } = new List<Item>();
    public int CoinReward { get; set; }
    public string NarrativeResult { get; set; }
}

/// <summary>
/// Relationship delta for a specific NPC in emergency outcomes.
/// DOMAIN COLLECTION PRINCIPLE: Used in List instead of Dictionary.
/// </summary>
public class NPCRelationshipDelta
{
    public NPC Npc { get; set; }
    public int Delta { get; set; }
}
