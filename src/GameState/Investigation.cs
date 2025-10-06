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
    
    // Location assignment (Mental/Physical goals)
    public string LocationId { get; set; }
    public string SpotId { get; set; }
    
    // NPC assignment (Social goals)
    public string NpcId { get; set; }
    public string RequestId { get; set; }
    
    // Prerequisites for this goal to spawn
    public GoalRequirements Requirements { get; set; } = new GoalRequirements();
}
