
// Conversation rules for new 5-state system
public static class SocialRules
{
    public static Dictionary<ConnectionState, SocialStateRules> States = new Dictionary<ConnectionState, SocialStateRules>
    {
        { ConnectionState.DISCONNECTED, new SocialStateRules("Disconnected - conversation ends at -3 flow", 1, 3, -3, 1, 0, null, true) },
        { ConnectionState.GUARDED, new SocialStateRules("Guarded and anxious", 2, 4, 0, 1, 0, null, false) },
        { ConnectionState.NEUTRAL, new SocialStateRules("Balanced starting state", 2, 5, 0, 1, 0, null, false) },
        { ConnectionState.RECEPTIVE, new SocialStateRules("Open and receptive", 3, 5, 0, 1, 0, null, false) },
        { ConnectionState.TRUSTING, new SocialStateRules("Maximum positive connection", 3, 6, 0, 1, 0, null, false) }
    };

    public static ConnectionState DetermineInitialState(NPC npc, ObligationQueueManager queueManager = null)
    {
        // All conversations start in NEUTRAL
        return ConnectionState.NEUTRAL;
    }



}
