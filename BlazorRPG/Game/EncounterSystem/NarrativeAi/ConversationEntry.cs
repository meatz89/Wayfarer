public class ConversationEntry
{
    public string Role { get; set; }
    public string Content { get; set; }
    public MessageType Type { get; set; }
}

public enum MessageType
{
    System,
    Introduction,
    ChoiceGeneration,
    PlayerChoice,
    Narrative,
    MemoryUpdate
}