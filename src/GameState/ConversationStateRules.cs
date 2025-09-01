
// Conversation state rules
public class ConversationStateRules
{
    public string Description { get; set; }
    public int CardsOnListen { get; set; }
    public int MaxWeight { get; set; }
    public bool ChecksGoalDeck { get; set; }
    public int ComfortThreshold { get; set; }
    public int PatienceReduction { get; set; }
    public int AutoAdvanceDepth { get; set; }
    public EmotionalState? ListenTransition { get; set; }
    public bool ListenEndsConversation { get; set; }

    public ConversationStateRules(string description, int cardsOnListen, int maxWeight, bool checksGoalDeck, int comfortThreshold, int patienceReduction, int autoAdvanceDepth, EmotionalState? listenTransition, bool listenEndsConversation)
    {
        Description = description;
        CardsOnListen = cardsOnListen;
        MaxWeight = maxWeight;
        ChecksGoalDeck = checksGoalDeck;
        ComfortThreshold = comfortThreshold;
        PatienceReduction = patienceReduction;
        AutoAdvanceDepth = autoAdvanceDepth;
        ListenTransition = listenTransition;
        ListenEndsConversation = listenEndsConversation;
    }
}
