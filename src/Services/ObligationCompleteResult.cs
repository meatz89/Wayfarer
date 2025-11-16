/// <summary>
/// Result returned when an obligation is fully completed
/// Contains data for displaying the obligation complete modal
/// </summary>
public class ObligationCompleteResult
{
    public Obligation Obligation { get; set; }
    public string ObligationName { get; set; }
    public string CompletionNarrative { get; set; }
    public ObligationRewards Rewards { get; set; } = new ObligationRewards();
    // ObservationCards system eliminated - replaced by transparent resource competition
}

/// <summary>
/// Rewards granted on obligation completion
/// </summary>
public class ObligationRewards
{
    public int Coins { get; set; }
    public List<StatXPReward> XPRewards { get; set; } = new List<StatXPReward>();
    public List<NPCReputationReward> NPCReputation { get; set; } = new List<NPCReputationReward>();
}

/// <summary>
