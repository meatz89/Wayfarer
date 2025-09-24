
// Conversation rules for new 5-state system
public static class ConversationRules
{
    public static Dictionary<ConnectionState, ConversationStateRules> States = new Dictionary<ConnectionState, ConversationStateRules>
    {
        { ConnectionState.DISCONNECTED, new ConversationStateRules("Disconnected - conversation ends at -3 flow", 1, 3, -3, 1, 0, null, true) },
        { ConnectionState.GUARDED, new ConversationStateRules("Guarded and anxious", 2, 4, 0, 1, 0, null, false) },
        { ConnectionState.NEUTRAL, new ConversationStateRules("Balanced starting state", 2, 5, 0, 1, 0, null, false) },
        { ConnectionState.RECEPTIVE, new ConversationStateRules("Open and receptive", 3, 5, 0, 1, 0, null, false) },
        { ConnectionState.TRUSTING, new ConversationStateRules("Maximum positive connection", 3, 6, 0, 1, 0, null, false) }
    };

    public static ConnectionState DetermineInitialState(NPC npc, ObligationQueueManager queueManager = null)
    {
        // All conversations start in NEUTRAL
        return ConnectionState.NEUTRAL;
    }



}
