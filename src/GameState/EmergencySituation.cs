/// <summary>
/// Urgent situation demanding immediate player response.
/// Interrupts normal gameplay at sync points (time advancement, location entry).
/// </summary>
public class EmergencySituation
{
    // HIGHLANDER: NO Id property - EmergencySituation identified by object reference
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

    // State
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
    // HIGHLANDER: NO Id property - EmergencyResponse identified by object reference
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
    // HIGHLANDER: Object references ONLY, no NPCRelationshipDeltas with IDs
    public Dictionary<NPC, int> NPCRelationshipDeltas { get; set; } = new Dictionary<NPC, int>();
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    // HIGHLANDER: Object references ONLY, no SpawnedSituationIds
    public List<Situation> SpawnedSituations { get; set; } = new List<Situation>();
    // HIGHLANDER: Object references ONLY, no GrantedItemIds
    public List<Item> GrantedItems { get; set; } = new List<Item>();
    public int CoinReward { get; set; }
    public string NarrativeResult { get; set; }
}
