/// <summary>
/// Urgent situation demanding immediate player response.
/// Interrupts normal gameplay at sync points (time advancement, location entry).
/// </summary>
public class EmergencySituation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Trigger conditions
    public int? TriggerDay { get; set; }
    public int? TriggerSegment { get; set; }
    public List<string> TriggerLocationIds { get; set; } = new List<string>();
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
    public Dictionary<string, int> NPCRelationshipDeltas { get; set; } = new Dictionary<string, int>();
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    public List<string> SpawnedSituationIds { get; set; } = new List<string>();
    public List<string> GrantedItemIds { get; set; } = new List<string>();
    public int CoinReward { get; set; }
    public string NarrativeResult { get; set; }
}
