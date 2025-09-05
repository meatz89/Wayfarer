
// Conversation state rules
public class ConversationStateRules
{
    public string Description { get; set; }
    public int CardsOnListen { get; set; }
    public int MaxFocus { get; set; }
    public bool ChecksRequestDeck { get; set; }
    public int FlowThreshold { get; set; }
    public int PatienceReduction { get; set; }
    public int AutoAdvanceDepth { get; set; }
    public ConnectionState? ListenTransition { get; set; }
    public bool ListenEndsConversation { get; set; }

    public ConversationStateRules(string description, int cardsOnListen, int maxFocus, bool checksRequestDeck, int flowThreshold, int patienceReduction, int autoAdvanceDepth, ConnectionState? listenTransition, bool listenEndsConversation)
    {
        Description = description;
        CardsOnListen = cardsOnListen;
        MaxFocus = maxFocus;
        ChecksRequestDeck = checksRequestDeck;
        FlowThreshold = flowThreshold;
        PatienceReduction = patienceReduction;
        AutoAdvanceDepth = autoAdvanceDepth;
        ListenTransition = listenTransition;
        ListenEndsConversation = listenEndsConversation;
    }
}
