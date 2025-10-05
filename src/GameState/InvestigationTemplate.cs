using System.Collections.Generic;

/// <summary>
/// V3 Investigation Template - complete investigation definition
/// Contains 3-5 fixed phases with authored goals and structure
/// </summary>
public class InvestigationTemplate
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    public LocationPersonalityType Personality { get; init; } // or combination
    public List<LocationPersonalityType> PersonalityTypes { get; init; } = new List<LocationPersonalityType>(); // For hybrid
    public int ExposureThreshold { get; init; } // 6-12 typical
    public int TimeLimit { get; init; } = 0; // 0 = no hard limit, >0 = hard Time Segment cap

    public List<InvestigationPhase> Phases { get; init; } = new List<InvestigationPhase>();

    // Observation cards created by major discoveries
    public List<InvestigationObservationReward> ObservationCardRewards { get; init; } = new List<InvestigationObservationReward>();
}

public class InvestigationObservationReward
{
    public string DiscoveryId { get; set; } // Which discovery triggers this
    public string NpcId { get; set; } // Which NPC gets the card
    public string CardId { get; set; } // Observation card ID to generate
}
