public enum BasicActionTypes
{
    Rest,

    // Physical Actions define direct interaction with the world:
    Labor, // Directed physical effort. (Reward Type: Wealth/Resources)
    Gather, // Collecting/taking resources. (Reward Type: Resources)
    Travel, // Traversing/positioning. (Reward Type: Position/State Change)

    // Social Actions handle character interactions:
    Mingle, // Casual interaction. (Reward Type: Relationships/Social Energy)
    Persuade, // Directed influence. (Reward Type: Relationships/State Change)
    Perform, // Entertainment/display. (Reward Type: Reputation/Social Energy)

    // Mental Actions cover intellectual activities:
    Observe, // Directed observation. (Reward Type: Knowledge/Opportunities)
    Study, // Focused learning. (Reward Type: Knowledge/Skills)
    Reflect, // Processing/rest. (Reward Type: State Change/Energy Restoration)
    Recover,
    Discuss,
}

