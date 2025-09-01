public enum AtmosphereType
{
    // Standard (30% of normal cards can set these)
    Neutral,
    Prepared,    // +1 weight capacity
    Receptive,   // +1 card on LISTEN
    Focused,     // +20% success all cards
    Patient,     // Actions cost 0 patience
    Volatile,    // All comfort changes ±1
    Final,       // Any failure ends conversation

    // Observation-only (unique effects)
    Informed,     // Next card cannot fail
    Exposed,      // Double comfort changes
    Synchronized, // Next effect happens twice
    Pressured     // -1 card on LISTEN
}