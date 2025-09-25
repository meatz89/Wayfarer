
// Conversation outcome - updated for 4-resource system
public class ConversationOutcome
{
    public bool Success { get; set; }
    public int FinalMomentum { get; set; }
    public int FinalDoubt { get; set; }
    public int FinalInitiative { get; set; }
    public int FinalCadence { get; set; }
    public ConnectionState FinalState { get; set; } // DEPRECATED: Will be removed in favor of resource-based outcomes
    public int TokensEarned { get; set; }
    public string Reason { get; set; }
    public bool RequestAchieved { get; set; }

    // Legacy compatibility property - maps to FinalMomentum
    [Obsolete("Use FinalMomentum instead")]
    public int FinalFlow
    {
        get => FinalMomentum;
        set => FinalMomentum = value;
    }
}
