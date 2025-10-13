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
    /// Rewards granted when investigation is completed
    /// </summary>
    public InvestigationRewards CompletionRewards { get; set; } = new InvestigationRewards();

    /// <summary>
    /// Observation cards unlocked on completion
    /// </summary>
    public List<ObservationCardReward> ObservationCardRewards { get; set; } = new List<ObservationCardReward>();
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

    // Prerequisites for this phase to complete
    public GoalRequirements Requirements { get; set; } = new GoalRequirements();

    // Rewards granted on completion
    public PhaseCompletionReward CompletionReward { get; set; }
}

/// <summary>
/// Rewards granted when phase completes
/// </summary>
public class PhaseCompletionReward
{
    public List<string> KnowledgeGranted { get; set; } = new List<string>();
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
/// Different trigger types use different prerequisite fields
/// </summary>
public class InvestigationPrerequisites
{
    /// <summary>
    /// Required location (LocationId is globally unique)
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// Required knowledge IDs (ConversationalDiscovery)
    /// </summary>
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Required item IDs (ItemDiscovery)
    /// </summary>
    public List<string> RequiredItems { get; set; } = new List<string>();

    /// <summary>
    /// Required obligation ID (ObligationTriggered)
    /// </summary>
    public string RequiredObligation { get; set; }
}
