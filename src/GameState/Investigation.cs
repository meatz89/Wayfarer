using System.Collections.Generic;

/// <summary>
/// Investigation template - contains metadata and phase definitions for goal creation
/// Investigation does NOT spawn tactical sessions directly - it creates LocationGoals/NPCGoals
/// which are evaluated by the existing goal system
/// </summary>
public class Investigation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CompletionNarrative { get; set; } // Narrative shown when investigation completes

    /// <summary>
    /// Intro action defines how investigation is discovered and activated
    /// Player must complete intro action to move investigation from Discovered → Active
    /// </summary>
    public InvestigationIntroAction IntroAction { get; set; }

    /// <summary>
    /// Color code for UI grouping and visual distinction
    /// </summary>
    public string ColorCode { get; set; }

    /// <summary>
    /// Phase definitions - used to create goals dynamically when prerequisites met
    /// </summary>
    public List<InvestigationPhaseDefinition> PhaseDefinitions { get; set; } = new List<InvestigationPhaseDefinition>();

    /// <summary>
    /// Type of obligation - determines pressure and mechanics
    /// NPCCommissioned: Has patron, deadline, failure consequences
    /// SelfDiscovered: No patron, no deadline, pure exploration
    /// </summary>
    public InvestigationObligationType ObligationType { get; set; } = InvestigationObligationType.SelfDiscovered;

    /// <summary>
    /// NPC who commissioned this investigation (if NPCCommissioned type)
    /// Null for SelfDiscovered investigations
    /// </summary>
    public string PatronNpcId { get; set; }

    /// <summary>
    /// Absolute segment number when investigation must be completed (if NPCCommissioned)
    /// Failure to complete by deadline damages relationship with patron
    /// Null for SelfDiscovered investigations
    /// </summary>
    public int? DeadlineSegment { get; set; }

    /// <summary>
    /// Coins granted when investigation completes (NPCCommissioned type)
    /// </summary>
    public int CompletionRewardCoins { get; set; } = 0;

    /// <summary>
    /// Items granted when investigation completes (equipment IDs)
    /// </summary>
    public List<string> CompletionRewardItems { get; set; } = new List<string>();

    /// <summary>
    /// Player stat XP rewards granted when investigation completes
    /// </summary>
    public List<StatXPReward> CompletionRewardXP { get; set; } = new List<StatXPReward>();

    /// <summary>
    /// New obligations spawned when investigation completes
    /// References to other Investigation IDs in GameWorld.Investigations
    /// </summary>
    public List<string> SpawnedObligationIds { get; set; } = new List<string>();

    /// <summary>
    /// Tracks whether this investigation failed to meet deadline
    /// Set to true when ApplyDeadlineConsequences is called
    /// </summary>
    public bool IsFailed { get; set; } = false;

    // ObservationCardRewards system eliminated - replaced by transparent resource competition
}

/// <summary>
/// Phase definition - references an existing goal from GameWorld.Goals
/// When prerequisites met, investigation system looks up goal and adds to ActiveGoals
/// </summary>
public class InvestigationPhaseDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CompletionNarrative { get; set; } // Narrative shown when investigation completes
    public string OutcomeNarrative { get; set; } // Narrative shown when goal completes

    // Rewards granted on completion
    // GoalRequirements system eliminated - phases progress through actual goal completion tracking
    public PhaseCompletionReward CompletionReward { get; set; }
}

/// <summary>
/// Rewards granted when phase completes
/// </summary>
public class PhaseCompletionReward
{
    /// <summary>
    /// Understanding points granted (1-3)
    /// Cumulative Mental expertise resource (0-10 max)
    /// Replaces Knowledge tokens (boolean gates eliminated)
    /// </summary>
    public int UnderstandingReward { get; set; } = 0;

    public List<ObstacleSpawnInfo> ObstaclesSpawned { get; set; } = new List<ObstacleSpawnInfo>();
}

/// <summary>
/// Defines where and what obstacle to spawn as investigation phase reward
/// </summary>
public class ObstacleSpawnInfo
{
    public ObstacleSpawnTargetType TargetType { get; set; }
    public string TargetEntityId { get; set; }
    public Obstacle Obstacle { get; set; }
}

/// <summary>
/// Type of entity where obstacle should be spawned
/// </summary>
public enum ObstacleSpawnTargetType
{
    Location,
    Route,
    NPC
}

/// <summary>
/// Investigation intro action - defines discovery trigger and activation mechanics
/// Simple RPG quest acceptance pattern: Show button → Player clicks → Modal displays → Accept quest → Activate investigation
/// NO CHALLENGE - intro action immediately activates investigation and spawns Phase 1
/// </summary>
public class InvestigationIntroAction
{
    /// <summary>
    /// Type of trigger that makes this investigation discoverable
    /// </summary>
    public DiscoveryTriggerType TriggerType { get; set; }

    /// <summary>
    /// Prerequisites that must be met for discovery trigger to fire
    /// </summary>
    public InvestigationPrerequisites TriggerPrerequisites { get; set; }

    /// <summary>
    /// Button text shown to player ("Search for safe entry to the mill")
    /// </summary>
    public string ActionText { get; set; }

    /// <summary>
    /// Location where intro button appears (LocationId is globally unique)
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// Narrative shown in modal when button clicked (quest acceptance text)
    /// </summary>
    public string IntroNarrative { get; set; }

    /// <summary>
    /// Rewards granted when player accepts quest
    /// Typically spawns Phase 1 obstacle to begin investigation
    /// </summary>
    public PhaseCompletionReward CompletionReward { get; set; }
}

/// <summary>
/// Prerequisites for investigation discovery trigger
/// Resource-based pattern: Investigations visible when player present at location
/// NO boolean gates - all investigations immediately discoverable
/// </summary>
public class InvestigationPrerequisites
{
    /// <summary>
    /// Required location (LocationId is globally unique)
    /// Investigation becomes discoverable when player enters this location
    /// </summary>
    public string LocationId { get; set; }
}

/// <summary>
/// Player stat XP reward - strongly typed replacement for Dictionary<string, int>
/// Used in investigation completion rewards
/// </summary>
public class StatXPReward
{
    public PlayerStatType Stat { get; set; }
    public int XPAmount { get; set; }
}

/// <summary>
/// NPC reputation reward - strongly typed replacement for Dictionary<string, int>
/// Used in investigation completion rewards
/// </summary>
public class NPCReputationReward
{
    public string NpcId { get; set; }
    public int ReputationChange { get; set; }
}
