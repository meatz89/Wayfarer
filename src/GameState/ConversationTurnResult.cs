
// Result of processing a conversation turn
public class ConversationTurnResult
{
    public bool Success { get; set; }
    public ConnectionState NewState { get; set; }
    public string NPCResponse { get; set; }
    public int? FlowChange { get; set; }
    public int? OldFlow { get; set; }
    public int? NewFlow { get; set; }
    public int? PatienceRemaining { get; set; }
    public List<CardInstance> DrawnCards { get; set; }
    public List<CardInstance> RemovedCards { get; set; }
    public List<CardInstance> PlayedCards { get; set; }
    public CardPlayResult CardPlayResult { get; set; }
    public bool ExchangeAccepted { get; set; }
    public NarrativeOutput Narrative { get; set; }  // Full narrative output for this turn

    public ConversationTurnResult()
    {
        DrawnCards = new List<CardInstance>();
        RemovedCards = new List<CardInstance>();
        PlayedCards = new List<CardInstance>();
    }
}
