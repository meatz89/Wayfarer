public class ConversationResult
{
    public bool Success { get; set; }
    public AIResponse AIResponse { get; set; }
    public string ConversationEndMessage { get; set; }
    public ConversationContext ConversationContext { get; set; }
    public PostConversationEvolutionResult PostConversationEvolution { get; set; }
    public IEnumerable<ProposedChange> ProposedChanges { get; set; }
}