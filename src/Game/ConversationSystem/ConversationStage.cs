public class ConversationStage
{
    public int StageNumber { get; set; }
    public string Description { get; set; }
    public List<ConversationChoice> Options { get; set; } = new List<ConversationChoice>();
    public bool IsCompleted { get; set; }
}
