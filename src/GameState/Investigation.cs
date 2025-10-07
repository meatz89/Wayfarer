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
/// Phase definition - template for creating a LocationGoal or NPCGoal
/// Contains all information needed to spawn a goal when prerequisites are met
/// </summary>
public class InvestigationPhaseDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CompletionNarrative { get; set; } // Narrative shown when investigation completes
    public string Goal { get; set; } // Goal text
    public string OutcomeNarrative { get; set; } // Narrative shown when goal completes
    
    public TacticalSystemType SystemType { get; set; }
    public string ChallengeTypeId { get; set; }

    // Spot assignment (SpotId is globally unique)
    public string SpotId { get; set; }
    
    // NPC assignment (Social goals)
    public string NpcId { get; set; }
    public string RequestId { get; set; }

    // Prerequisites for this goal to spawn
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
}

/// <summary>
/// Investigation intro action - defines discovery trigger and activation mechanics
/// Intro action must be completed to move investigation from Discovered → Active
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
    /// Action text shown to player ("Notice the silent waterwheel")
    /// </summary>
    public string ActionText { get; set; }

    /// <summary>
    /// Which tactical system for intro challenge (Mental/Physical/Social)
    /// </summary>
    public TacticalSystemType SystemType { get; set; }

    /// <summary>
    /// Specific challenge type ID (mental_challenge, physical_challenge, social_challenge)
    /// </summary>
    public string ChallengeTypeId { get; set; }

    /// <summary>
    /// Spot where intro action appears (SpotId is globally unique)
    /// </summary>
    public string SpotId { get; set; }

    /// <summary>
    /// NPC for intro conversation (Social only)
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// Request ID for intro conversation (Social only)
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// Narrative shown after completing intro action
    /// </summary>
    public string IntroNarrative { get; set; }
}

/// <summary>
/// Prerequisites for investigation discovery trigger
/// Different trigger types use different prerequisite fields
/// </summary>
public class InvestigationPrerequisites
{
    /// <summary>
    /// Required spot (SpotId is globally unique)
    /// </summary>
    public string SpotId { get; set; }

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
