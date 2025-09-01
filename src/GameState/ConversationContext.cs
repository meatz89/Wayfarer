
// Conversation context for UI
public class ConversationContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }
    public ConversationType Type { get; set; }
    public EmotionalState InitialState { get; set; }
    public ConversationSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public int AttentionSpent { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
    public List<DeliveryObligation> LettersCarriedForNpc { get; set; }
}
