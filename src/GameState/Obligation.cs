/// <summary>
/// Obligation template - contains metadata and phase definitions for situation creation
/// Obligation does NOT spawn tactical sessions directly - it creates LocationSituations/NPCSituations
/// which are evaluated by the existing situation system
/// </summary>
public class Obligation
{
public string Id { get; set; }
public string Name { get; set; }
public string Description { get; set; }
public string CompletionNarrative { get; set; } // Narrative shown when obligation completes

/// <summary>
/// Intro action defines how obligation is discovered and activated
/// Player must complete intro action to move obligation from Discovered → Active
/// </summary>
public ObligationIntroAction IntroAction { get; set; }

/// <summary>
/// Color code for UI grouping and visual distinction
/// </summary>
public string ColorCode { get; set; }

/// <summary>
/// Phase definitions - used to create situations dynamically when prerequisites met
/// </summary>
public List<ObligationPhaseDefinition> PhaseDefinitions { get; set; } = new List<ObligationPhaseDefinition>();

/// <summary>
/// Type of obligation - determines pressure and mechanics
/// NPCCommissioned: Has patron, deadline, failure consequences
/// SelfDiscovered: No patron, no deadline, pure exploration
/// </summary>
public ObligationObligationType ObligationType { get; set; } = ObligationObligationType.SelfDiscovered;

/// <summary>
/// NPC who commissioned this obligation (if NPCCommissioned type)
/// Null for SelfDiscovered obligations
/// </summary>
public string PatronNpcId { get; set; }

/// <summary>
/// Object reference to patron NPC (for runtime navigation)
/// Populated at initialization time from PatronNpcId
/// </summary>
[System.Text.Json.Serialization.JsonIgnore]
public NPC PatronNpc { get; set; }

/// <summary>
/// Absolute segment number when obligation must be completed (if NPCCommissioned)
/// Failure to complete by deadline damages relationship with patron
/// Null for SelfDiscovered obligations
/// </summary>
public int? DeadlineSegment { get; set; }

/// <summary>
/// Coins granted when obligation completes (NPCCommissioned type)
/// </summary>
public int CompletionRewardCoins { get; set; } = 0;

/// <summary>
/// Items granted when obligation completes (equipment IDs)
/// </summary>
public List<string> CompletionRewardItems { get; set; } = new List<string>();

/// <summary>
/// Player stat XP rewards granted when obligation completes
/// </summary>
public List<StatXPReward> CompletionRewardXP { get; set; } = new List<StatXPReward>();

/// <summary>
/// New obligations spawned when obligation completes
/// References to other Obligation IDs in GameWorld.Obligations
/// </summary>
public List<string> SpawnedObligationIds { get; set; } = new List<string>();

/// <summary>
/// Tracks whether this obligation failed to meet deadline
/// Set to true when ApplyDeadlineConsequences is called
/// </summary>
public bool IsFailed { get; set; } = false;

// ObservationCardRewards system eliminated - replaced by transparent resource competition
}

/// <summary>
/// Phase definition - references an existing situation from GameWorld.Situations
/// When prerequisites met, obligation system looks up situation and adds to ActiveSituations
/// </summary>
public class ObligationPhaseDefinition
{
public string Id { get; set; }
public string Name { get; set; }
public string Description { get; set; }
public string CompletionNarrative { get; set; } // Narrative shown when obligation completes
public string OutcomeNarrative { get; set; } // Narrative shown when situation completes

// Rewards granted on completion
// SituationRequirements system eliminated - phases progress through actual situation completion tracking
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

/// <summary>
/// Scene-Situation architecture: Scenes to spawn from templates
/// Replaces legacy ScenesSpawned (which had full Scene objects)
/// Now uses SceneTemplate IDs with placement relations
/// </summary>
public List<SceneSpawnReward> ScenesToSpawn { get; set; } = new List<SceneSpawnReward>();
}

/// <summary>
/// Obligation intro action - defines discovery trigger and activation mechanics
/// Simple RPG quest acceptance pattern: Show button → Player clicks → Modal displays → Accept quest → Activate obligation
/// NO CHALLENGE - intro action immediately activates obligation and spawns Phase 1
/// </summary>
public class ObligationIntroAction
{
/// <summary>
/// Type of trigger that makes this obligation discoverable
/// </summary>
public DiscoveryTriggerType TriggerType { get; set; }

/// <summary>
/// Prerequisites that must be met for discovery trigger to fire
/// </summary>
public ObligationPrerequisites TriggerPrerequisites { get; set; }

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
/// Typically spawns Phase 1 scene to begin obligation
/// </summary>
public PhaseCompletionReward CompletionReward { get; set; }
}

/// <summary>
/// Prerequisites for obligation discovery trigger
/// Resource-based pattern: Obligations visible when player present at location
/// NO boolean gates - all obligations immediately discoverable
/// </summary>
public class ObligationPrerequisites
{
/// <summary>
/// Required location (LocationId is globally unique)
/// Obligation becomes discoverable when player enters this location
/// </summary>
public string LocationId { get; set; }
}

/// <summary>
/// Player stat XP reward - strongly typed replacement for Dictionary<string, int>
/// Used in obligation completion rewards
/// </summary>
public class StatXPReward
{
public PlayerStatType Stat { get; set; }
public int XPAmount { get; set; }
}

/// <summary>
/// NPC reputation reward - strongly typed replacement for Dictionary<string, int>
/// Used in obligation completion rewards
/// </summary>
public class NPCReputationReward
{
public string NpcId { get; set; }
public int ReputationChange { get; set; }
}
