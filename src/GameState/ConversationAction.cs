
// Conversation action
public class ConversationAction
{
    public ActionType ActionType { get; set; }
    public HashSet<CardInstance> SelectedCards { get; set; }
    public bool IsAvailable { get; set; }
    public List<CardInstance> AvailableCards { get; set; }
    
    public ConversationAction()
    {
        SelectedCards = new HashSet<CardInstance>();
        AvailableCards = new List<CardInstance>();
    }
}
