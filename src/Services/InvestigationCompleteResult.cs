using System.Collections.Generic;

/// <summary>
/// Result returned when an investigation is fully completed
/// Contains data for displaying the investigation complete modal
/// </summary>
public class InvestigationCompleteResult
{
    public string InvestigationId { get; set; }
    public string InvestigationName { get; set; }
    public string CompletionNarrative { get; set; }
    public InvestigationRewards Rewards { get; set; } = new InvestigationRewards();
    // ObservationCards system eliminated - replaced by transparent resource competition
}

/// <summary>
/// Rewards granted on investigation completion
/// </summary>
public class InvestigationRewards
{
    public int Coins { get; set; }
    public List<StatXPReward> XPRewards { get; set; } = new List<StatXPReward>();
    public List<NPCReputationReward> NPCReputation { get; set; } = new List<NPCReputationReward>();
}

/// <summary>
