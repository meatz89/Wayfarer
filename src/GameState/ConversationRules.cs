
// Conversation rules for new 5-state system
public static class ConversationRules
{
    public static Dictionary<EmotionalState, ConversationStateRules> States = new Dictionary<EmotionalState, ConversationStateRules>
    {
        { EmotionalState.DESPERATE, new ConversationStateRules("Desperate - conversation ends at -3 flow", 1, 3, true, -3, 1, 0, null, true) },
        { EmotionalState.TENSE, new ConversationStateRules("Tense and anxious", 2, 4, false, 0, 1, 0, null, false) },
        { EmotionalState.NEUTRAL, new ConversationStateRules("Balanced starting state", 2, 5, false, 0, 1, 0, null, false) },
        { EmotionalState.OPEN, new ConversationStateRules("Open and receptive", 3, 5, false, 0, 1, 0, null, false) },
        { EmotionalState.CONNECTED, new ConversationStateRules("Maximum positive connection", 3, 6, false, 0, 1, 0, null, false) }
    };

    public static EmotionalState DetermineInitialState(NPC npc, ObligationQueueManager queueManager = null)
    {
        // All conversations start in NEUTRAL
        return EmotionalState.NEUTRAL;
    }

    public static EmotionalState TransitionState(EmotionalState current, int flowChange)
    {
        // Linear progression: DESPERATE ← TENSE ← NEUTRAL → OPEN → CONNECTED
        if (flowChange >= 3)
        {
            return current switch
            {
                EmotionalState.DESPERATE => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.CONNECTED,
                EmotionalState.CONNECTED => EmotionalState.CONNECTED, // Stay at max
                _ => EmotionalState.NEUTRAL
            };
        }
        else if (flowChange <= -3)
        {
            return current switch
            {
                EmotionalState.CONNECTED => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.DESPERATE,
                EmotionalState.DESPERATE => EmotionalState.DESPERATE, // Stay - ends conversation
                _ => EmotionalState.NEUTRAL
            };
        }

        return current; // No transition
    }

    public static string GetStateEffects(EmotionalState state)
    {
        return States.ContainsKey(state) ? States[state].Description : "Unknown state";
    }

    public static Dictionary<EmotionalState, string> GetAllStateEffects()
    {
        Dictionary<EmotionalState, string> dict = new Dictionary<EmotionalState, string>();
        foreach (KeyValuePair<EmotionalState, ConversationStateRules> kvp in States)
        {
            dict[kvp.Key] = kvp.Value.Description;
        }
        return dict;
    }
}
