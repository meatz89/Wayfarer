
// Conversation memento for save/load
public class ConversationMemento
{
    public string NpcId { get; set; }
    public ConversationType ConversationType { get; set; }
    public ConnectionState CurrentState { get; set; }
    public int CurrentFlow { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool RequestCardDrawn { get; set; }
    public int? RequestUrgencyCounter { get; set; }
    public bool RequestCardPlayed { get; set; }
    public List<string> HandCardIds { get; set; }
    public List<string> DeckCardIds { get; set; }
}
