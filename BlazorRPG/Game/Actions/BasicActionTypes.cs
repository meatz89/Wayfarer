public enum BasicActionTypes
{
    // Global Actions
    GlobalStatus, // View player state, active quests, relationships
    GlobalTravel, // Move between connected, unlocked locations
    GlobalRest,   // Restore energy, advance time

    // Location Actions
    Investigate,  // Discover and understand location elements
    Labor,        // Location-specific work for resources
    Gather,       // Collect location-specific resources/items
    Study,        // Learn location-specific information/skills
    Practice,     // Improve specific skills

    // Character Actions
    Observe,      // Watch and learn about characters indirectly
    Discuss,      // General conversation and information exchange
    Confide,      // Share personal matters and deepen relationships
    Assist,       // Help characters with their work/tasks
    Trade,        // Trade or gift items
}