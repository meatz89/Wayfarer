public enum BasicActionTypes
{
    Rest,

    // Physical Actions define direct interaction with the world:
    Labor, // Directed physical effort. (Reward Type: Wealth/Resources)
    Gather, // Collecting/taking resources. (Reward Type: Resources)
    Craft, // Creating/combining resources. (Reward Type: Resources/Items)
    Move, // Traversing/positioning. (Reward Type: Position/State Change)

    // Social Actions handle character interactions:
    Mingle, // Casual interaction. (Reward Type: Relationships/Social Energy)
    Trade, // Formal exchange. (Reward Type: Wealth/Resources)
    Persuade, // Directed influence. (Reward Type: Relationships/State Change)
    Perform, // Entertainment/display. (Reward Type: Reputation/Social Energy)

    // Mental Actions cover intellectual activities:
    Investigate, // Directed observation. (Reward Type: Knowledge/Opportunities)
    Study, // Focused learning. (Reward Type: Knowledge/Skills)
    Plan, // Strategic thinking. (Reward Type: State Change/Opportunities)
    Reflect, // Processing/rest. (Reward Type: State Change/Energy Restoration)
}

