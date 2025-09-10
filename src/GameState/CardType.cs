public enum CardType
{
    Conversation,  // Standard conversation cards (default)
    Letter,        // Goal cards that create obligations AND letter items when accepted
    Promise,       // Goal cards that create/modify obligations WITHOUT letter items
    BurdenGoal,    // Goal cards that enable "Make Amends" conversations to remove burdens
    Observation    // Observation cards from location discoveries
}
