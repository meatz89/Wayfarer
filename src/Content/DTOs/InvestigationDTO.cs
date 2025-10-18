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

    // NEW: Intro action for discovery system
    public InvestigationIntroActionDTO Intro { get; set; }

    // NEW: Color code for UI grouping
    public string ColorCode { get; set; }

    // Core Loop: Obligation system
    public string ObligationType { get; set; } = "SelfDiscovered";
    public string PatronNpcId { get; set; }
    public int? DeadlineSegment { get; set; }

    // Completion rewards (NPCCommissioned investigations)
    public int CompletionRewardCoins { get; set; } = 0;
    public List<string> CompletionRewardItems { get; set; } = new List<string>();
    public Dictionary<string, int> CompletionRewardXP { get; set; } = new Dictionary<string, int>(); // JSON: {"Insight": 10, "Rapport": 5}
    public List<string> SpawnedInvestigationIds { get; set; } = new List<string>();

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
    public string OutcomeNarrative { get; set; }

    public PhaseRequirementsDTO Requirements { get; set; } = new PhaseRequirementsDTO();
    public PhaseCompletionRewardDTO CompletionReward { get; set; }
}

/// <summary>
/// DTO for Phase Requirements
/// PRINCIPLE 4: All boolean gate requirements eliminated
/// Phases complete based on Understanding accumulation, not prerequisite checks
/// </summary>
public class PhaseRequirementsDTO
{
    public Dictionary<string, int> DiscoveryQuantities { get; set; } = new Dictionary<string, int>();
    public List<string> SpecificDiscoveries { get; set; } = new List<string>();
    // CompletedGoals, Equipment, Knowledge system eliminated - all boolean gates removed
}

/// <summary>
/// DTO for Phase Completion Reward
/// </summary>
public class PhaseCompletionRewardDTO
{
    public string Narrative { get; set; }
    public List<string> DiscoveriesGranted { get; set; } = new List<string>();
    public int UnderstandingReward { get; set; } = 0; // Replaces KnowledgeGranted - 0-10 scale
    public string UnlocksPhaseId { get; set; }
    public List<ObstacleSpawnInfoDTO> ObstaclesSpawned { get; set; } = new List<ObstacleSpawnInfoDTO>();
}

/// <summary>
/// DTO for obstacle spawn information in investigation phase rewards
/// </summary>
public class ObstacleSpawnInfoDTO
{
    public string TargetType { get; set; } // "Location", "Route", or "NPC"
    public string TargetEntityId { get; set; }
    public ObstacleDTO Obstacle { get; set; }
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

/// <summary>
/// DTO for Investigation Intro Action
/// Defines discovery trigger and activation mechanics
/// Simple RPG quest acceptance pattern - no challenge, just accept and begin
/// </summary>
public class InvestigationIntroActionDTO
{
    public string TriggerType { get; set; } // "ImmediateVisibility", "EnvironmentalObservation", etc.
    public InvestigationPrerequisitesDTO TriggerPrerequisites { get; set; }
    public string ActionText { get; set; } // Button text: "Search for safe entry to the mill"
    public string LocationId { get; set; } // LocationId where intro button appears
    public string IntroNarrative { get; set; } // Modal narrative when button clicked
    public PhaseCompletionRewardDTO CompletionReward { get; set; } // Spawns Phase 1 obstacle when intro completes
}

/// <summary>
/// DTO for Investigation Prerequisites
/// Used for both intro triggers and phase requirements
/// </summary>
public class InvestigationPrerequisitesDTO
{
    public string LocationId { get; set; } // LocationId is globally unique
    // PRINCIPLE 4: Knowledge system, RequiredItems, and RequiredObligation eliminated
    // All boolean gate prerequisites removed - investigations visible based on narrative context only
}
