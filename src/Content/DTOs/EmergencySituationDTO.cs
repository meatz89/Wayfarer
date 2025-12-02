/// <summary>
/// DTO for EmergencySituation - urgent situation demanding immediate player response.
/// Interrupts normal gameplay at sync points (time advancement, location entry).
/// </summary>
public class EmergencySituationDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Day on which this emergency triggers (null = trigger by other conditions)
    /// </summary>
    public int? TriggerDay { get; set; }

    /// <summary>
    /// Time segment at which this emergency triggers (null = any segment)
    /// </summary>
    public int? TriggerSegment { get; set; }

    /// <summary>
    /// Location IDs where this emergency can trigger (empty = any location)
    /// </summary>
    public List<string> TriggerLocationIds { get; set; } = new List<string>();

    /// <summary>
    /// How many time segments the player has to respond before auto-resolving
    /// </summary>
    public int ResponseWindowSegments { get; set; }

    /// <summary>
    /// Response options available to the player
    /// </summary>
    public List<EmergencyResponseDTO> Responses { get; set; } = new List<EmergencyResponseDTO>();

    /// <summary>
    /// Outcome if the player ignores or fails to respond in time
    /// </summary>
    public EmergencyOutcomeDTO IgnoreOutcome { get; set; }

    /// <summary>
    /// Whether this emergency has been triggered (state tracking)
    /// </summary>
    public bool IsTriggered { get; set; }

    /// <summary>
    /// Whether this emergency has been resolved (state tracking)
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Segment at which this emergency was triggered (state tracking)
    /// </summary>
    public int? TriggeredAtSegment { get; set; }
}

/// <summary>
/// A response option for an emergency situation.
/// Costs resources immediately, grants outcomes, spawns follow-up content.
/// </summary>
public class EmergencyResponseDTO
{
    public string Id { get; set; }
    public string ResponseText { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Stamina cost to choose this response
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Health cost to choose this response
    /// </summary>
    public int HealthCost { get; set; }

    /// <summary>
    /// Coin cost to choose this response
    /// </summary>
    public int CoinCost { get; set; }

    /// <summary>
    /// Time cost in segments to choose this response
    /// </summary>
    public int TimeCost { get; set; }

    /// <summary>
    /// Outcome of choosing this response
    /// </summary>
    public EmergencyOutcomeDTO Outcome { get; set; }
}

/// <summary>
/// Consequences of responding to (or ignoring) an emergency.
/// </summary>
public class EmergencyOutcomeDTO
{
    /// <summary>
    /// General reputation change across all NPCs
    /// </summary>
    public int RelationshipDelta { get; set; }

    /// <summary>
    /// Specific relationship changes with individual NPCs
    /// </summary>
    public List<NPCRelationshipDeltaEntry> NPCRelationshipDeltas { get; set; } = new List<NPCRelationshipDeltaEntry>();

    /// <summary>
    /// Knowledge tokens granted by this outcome
    /// </summary>
    public List<string> GrantedKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Situation IDs spawned by this outcome
    /// </summary>
    public List<string> SpawnedSituationIds { get; set; } = new List<string>();

    /// <summary>
    /// Item IDs granted by this outcome
    /// </summary>
    public List<string> GrantedItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Coin reward (or penalty if negative) from this outcome
    /// </summary>
    public int CoinReward { get; set; }

    /// <summary>
    /// Narrative description of the result shown to player
    /// </summary>
    public string NarrativeResult { get; set; }
}
