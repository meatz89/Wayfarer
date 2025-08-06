using System;

/// <summary>
/// Tags indicating various pressure states in a scene
/// </summary>
public enum PressureTag
{
    DEADLINE_IMMINENT,     // Less than 3 hours to deadline
    QUEUE_OVERFLOW,        // 6+ letters in queue
    DEBT_PRESENT,         // Any negative tokens
    DEBT_CRITICAL,        // -3 or worse in any token type
    OBLIGATION_ACTIVE,    // Standing obligation affecting choices
    PATRON_WATCHING       // Patron has expectations
}

/// <summary>
/// Tags indicating relationship states with current NPC
/// </summary>
public enum RelationshipTag
{
    TRUST_HIGH,           // 4+ trust tokens
    TRUST_NEGATIVE,       // Negative trust (betrayal)
    COMMERCE_ESTABLISHED, // 2+ commerce tokens
    COMMERCE_INDEBTED,    // Negative commerce (leverage)
    STATUS_RECOGNIZED,    // 3+ status tokens
    STATUS_SCORNED,       // Negative status (disgrace)
    SHADOW_COMPLICIT,     // Any shadow tokens
    SHADOW_THREATENED,    // Negative shadow (danger)
    FIRST_MEETING,        // Never interacted before
    STRANGER             // No tokens at all
}

/// <summary>
/// Tags indicating discovery opportunities
/// </summary>
public enum DiscoveryTag
{
    RUMOR_AVAILABLE,      // NPC has rumors to share
    ROUTE_UNKNOWN,        // Undiscovered route nearby
    NPC_HIDDEN,          // Hidden NPC could be revealed
    INFORMATION_HINTED,   // Information can be gleaned
    SECRET_PRESENT       // Secret knowledge available
}

/// <summary>
/// Tags indicating resource states
/// </summary>
public enum ResourceTag
{
    COINS_ABUNDANT,       // 20+ coins
    COINS_SUFFICIENT,     // 5-19 coins
    COINS_LOW,           // 1-4 coins
    COINS_NONE,          // 0 coins
    STAMINA_FULL,        // Max stamina
    STAMINA_RESTED,      // 70%+ stamina
    STAMINA_TIRED,       // 30-69% stamina
    STAMINA_EXHAUSTED,   // <30% stamina
    INVENTORY_FULL,      // No space for items
    INVENTORY_EMPTY      // No items carried
}

/// <summary>
/// Tags indicating atmospheric feelings at a location
/// </summary>
public enum FeelingTag
{
    // Temperature
    HEARTH_WARMED,       // Cozy warmth
    SUN_DRENCHED,        // Bright sunlight
    RAIN_SOAKED,         // Wet and dripping
    FROST_TOUCHED,       // Cold bite

    // Social atmosphere
    BUSTLING,            // Crowded and active
    INTIMATE,            // Quiet and close
    TENSE,              // Nervous energy
    CELEBRATORY,        // Festive mood
    HOSTILE,            // Dangerous atmosphere

    // Sensory
    ALE_SCENTED,        // Tavern smells
    SMOKE_FILLED,       // Hazy air
    MUSIC_DRIFTING,     // Background melodies
    SILENCE_HEAVY,      // Oppressive quiet

    // Emotional
    URGENCY_GNAWS,      // Time pressure feeling
    COMFORT_EMBRACES,   // Safe and welcoming
    MYSTERY_WHISPERS,   // Secrets in the air
    DANGER_LURKS       // Threat sensed
}