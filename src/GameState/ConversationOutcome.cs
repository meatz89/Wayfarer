
// Conversation outcome
public class ConversationOutcome
{
    public bool Success { get; set; }
    public int FinalComfort { get; set; }
    public EmotionalState FinalState { get; set; }
    public int TokensEarned { get; set; }
    public string Reason { get; set; }
    public bool GoalAchieved { get; set; }
    public int TotalComfort => FinalComfort;  // Alias for compatibility
}
