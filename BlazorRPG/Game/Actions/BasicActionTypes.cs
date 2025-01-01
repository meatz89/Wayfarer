public enum BasicActionTypes
{
    Wait,   // Advance time
    Rest,   // Restore energy, advance time

    // Location Actions
    Investigate,  // Discover and understand location elements
    Labor,        // Location-specific work for resources
    Gather,       // Collect location-specific resources/items
    Trade,        // Trade or gift items
    Mingle,

    // Character Actions
    Observe,      // Watch and learn about characters indirectly
    Discuss,      // General conversation and information exchange
    Confide,      // Share personal matters and deepen relationships
    Assist,       // Help characters with their work/tasks
}