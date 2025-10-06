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
    public List<ObservationCardReward> ObservationCards { get; set; } = new List<ObservationCardReward>();
}

/// <summary>
/// Rewards granted on investigation completion
/// </summary>
public class InvestigationRewards
{
    public int Coins { get; set; }
    public int DeductionXP { get; set; }
    public int EmpathyXP { get; set; }
    public Dictionary<string, int> NPCReputation { get; set; } = new Dictionary<string, int>();
}

/// <summary>
