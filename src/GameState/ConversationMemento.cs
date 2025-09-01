
// Conversation memento for save/load
public class ConversationMemento
{
    public string NpcId { get; set; }
    public ConversationType ConversationType { get; set; }
    public EmotionalState CurrentState { get; set; }
    public int CurrentComfort { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool GoalCardDrawn { get; set; }
    public int? GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public List<string> HandCardIds { get; set; }
    public List<string> DeckCardIds { get; set; }
}
