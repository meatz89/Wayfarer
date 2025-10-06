using System.Collections.Generic;

/// <summary>
/// DTO for Investigation Template - complete investigation definition
/// Maps to InvestigationTemplate domain model
/// </summary>
public class InvestigationDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Personality { get; set; } // or combination
    public List<string> PersonalityTypes { get; set; } = new List<string>(); // For hybrid
    public int ExposureThreshold { get; set; } // 6-12 typical
    public int TimeLimit { get; set; } = 0; // 0 = no hard limit, >0 = hard Time Segment cap
    public List<InvestigationPhaseDTO> Phases { get; set; } = new List<InvestigationPhaseDTO>();
    public List<InvestigationObservationRewardDTO> ObservationCardRewards { get; set; } = new List<InvestigationObservationRewardDTO>();
}

/// <summary>
/// DTO for Investigation Phase
/// </summary>
public class InvestigationPhaseDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public int ProgressThreshold { get; set; }
    public string SystemType { get; set; } // "Social", "Mental", or "Physical"
    public string ChallengeTypeId { get; set; } // Engagement type ID for Mental/Physical, or conversation type for Social

    // Location assignment (for Mental/Physical goals)
    public string LocationId { get; set; }
    public string SpotId { get; set; }

    // NPC assignment (for Social goals)
    public string NpcId { get; set; }
    public string RequestId { get; set; }

    public List<string> CardDeckIds { get; set; } = new List<string>();
    public PhaseRequirementsDTO Requirements { get; set; } = new PhaseRequirementsDTO();
    public PhaseCompletionRewardDTO CompletionReward { get; set; }
}

/// <summary>
/// DTO for Phase Requirements
/// </summary>
public class PhaseRequirementsDTO
{
    public List<int> CompletedPhases { get; set; } = new List<int>();
    public Dictionary<string, int> DiscoveryQuantities { get; set; } = new Dictionary<string, int>();
    public List<string> SpecificDiscoveries { get; set; } = new List<string>();
    public List<string> Equipment { get; set; } = new List<string>();
    public List<string> Knowledge { get; set; } = new List<string>();
}

/// <summary>
/// DTO for Phase Completion Reward
/// </summary>
public class PhaseCompletionRewardDTO
{
    public string Narrative { get; set; }
    public List<string> DiscoveriesGranted { get; set; } = new List<string>();
    public string UnlocksPhaseId { get; set; }
}

/// <summary>
/// DTO for Investigation Observation Reward
/// </summary>
public class InvestigationObservationRewardDTO
{
    public string DiscoveryId { get; set; }
    public string NpcId { get; set; }
    public string CardId { get; set; }
}
