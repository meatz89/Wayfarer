public enum BasicActionTypes
{
    Travel, // Traversing/positioning. (Reward Type: Position/State Change)
    Rest,

    Labor, // Physical/Physical (Reward Type: Wealth/Resources)
    Gather, // Physical/Focus (Reward Type: Items/Resources)
    Fight, // Physical/Social

    Study, // Focus/Focus. (Reward Type: Knowledge/Skills)
    Investigate, // Focus/Physical. (Reward Type: Knowledge/Opportunities)
    Analyze, // Focus/Social

    Discuss, // Social/Social (Reward Type: Relationships/Social Energy)
    Persuade, // Social/Focus (Reward Type: Relationships/State Change)
    Perform, // Social/Physical (Reward Type: Reputation/Social Energy)
}

