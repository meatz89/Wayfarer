public enum CardType
{
    Conversation,  // Social tactical system - standard conversation cards
    Mental,        // Mental tactical system - investigation/puzzle cards
    Physical,      // Physical tactical system - challenge/obstacle cards
    Request,       // Conversation subtype: Request cards that complete NPC goals
    Promise,       // Conversation subtype: Goal cards that create obligations
    Burden,        // Conversation subtype: Goal cards for "Make Amends"
    Observation    // Conversation subtype: Observation cards from discoveries
}
