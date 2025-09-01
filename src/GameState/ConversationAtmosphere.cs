public enum ConversationAtmosphere
{
    Neutral,      // No effect (default)
    Prepared,     // +1 weight capacity
    Receptive,    // +1 card on LISTEN
    Focused,      // +20% success
    Patient,      // 0 patience cost
    Volatile,     // ±1 comfort changes
    Final,        // Failure ends conversation
    Informed,     // Next card auto-succeeds (observation only)
    Exposed,      // Double comfort changes (observation only)
    Synchronized, // Next effect happens twice (observation only)
    Pressured     // -1 card on LISTEN (observation only)
}
