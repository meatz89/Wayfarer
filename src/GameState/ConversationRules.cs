
// Conversation rules for new 5-state system
public static class ConversationRules
{
    public static Dictionary<ConnectionState, ConversationStateRules> States = new Dictionary<ConnectionState, ConversationStateRules>
    {
        { ConnectionState.DISCONNECTED, new ConversationStateRules("Disconnected - conversation ends at -3 flow", 1, 3, true, -3, 1, 0, null, true) },
        { ConnectionState.GUARDED, new ConversationStateRules("Guarded and anxious", 2, 4, false, 0, 1, 0, null, false) },
        { ConnectionState.NEUTRAL, new ConversationStateRules("Balanced starting state", 2, 5, false, 0, 1, 0, null, false) },
        { ConnectionState.RECEPTIVE, new ConversationStateRules("Open and receptive", 3, 5, false, 0, 1, 0, null, false) },
        { ConnectionState.TRUSTING, new ConversationStateRules("Maximum positive connection", 3, 6, false, 0, 1, 0, null, false) }
    };

    public static ConnectionState DetermineInitialState(NPC npc, ObligationQueueManager queueManager = null)
    {
        // All conversations start in NEUTRAL
        return ConnectionState.NEUTRAL;
    }

    public static ConnectionState TransitionState(ConnectionState current, int flowChange)
    {
        // Linear progression: DISCONNECTED ← GUARDED ← NEUTRAL → OPEN → CONNECTED
        if (flowChange >= 3)
        {
            return current switch
            {
                ConnectionState.DISCONNECTED => ConnectionState.GUARDED,
                ConnectionState.GUARDED => ConnectionState.NEUTRAL,
                ConnectionState.NEUTRAL => ConnectionState.RECEPTIVE,
                ConnectionState.RECEPTIVE => ConnectionState.TRUSTING,
                ConnectionState.TRUSTING => ConnectionState.TRUSTING, // Stay at max
                _ => ConnectionState.NEUTRAL
            };
        }
        else if (flowChange <= -3)
        {
            return current switch
            {
                ConnectionState.TRUSTING => ConnectionState.RECEPTIVE,
                ConnectionState.RECEPTIVE => ConnectionState.NEUTRAL,
                ConnectionState.NEUTRAL => ConnectionState.GUARDED,
                ConnectionState.GUARDED => ConnectionState.DISCONNECTED,
                ConnectionState.DISCONNECTED => ConnectionState.DISCONNECTED, // Stay - ends conversation
                _ => ConnectionState.NEUTRAL
            };
        }

        return current; // No transition
    }

    public static string GetStateEffects(ConnectionState state)
    {
        return States.ContainsKey(state) ? States[state].Description : "Unknown state";
    }

    public static Dictionary<ConnectionState, string> GetAllStateEffects()
    {
        Dictionary<ConnectionState, string> dict = new Dictionary<ConnectionState, string>();
        foreach (KeyValuePair<ConnectionState, ConversationStateRules> kvp in States)
        {
            dict[kvp.Key] = kvp.Value.Description;
        }
        return dict;
    }
}
