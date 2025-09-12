
// Conversation outcome
public class ConversationOutcome
{
    public bool Success { get; set; }
    public int FinalFlow { get; set; }
    public ConnectionState FinalState { get; set; }
    public int TokensEarned { get; set; }
    public string Reason { get; set; }
    public bool RequestAchieved { get; set; }
}
