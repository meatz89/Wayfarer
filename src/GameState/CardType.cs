public enum CardType
{
    Conversation,  // Standard conversation cards (default)
    Request,       // Request cards that complete NPC goals and end conversations
    Promise,       // Goal cards that create/modify obligations WITHOUT letter items (including queue manipulation)
    Burden,        // Goal cards that enable "Make Amends" conversations to remove burdens
    Observation    // Observation cards from location discoveries
}
