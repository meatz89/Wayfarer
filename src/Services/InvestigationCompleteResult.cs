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
    public int DeductionXP { get; set; }
    public int EmpathyXP { get; set; }
    public Dictionary<string, int> NPCReputation { get; set; } = new Dictionary<string, int>();
}

/// <summary>
