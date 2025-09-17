namespace Wayfarer.GameState.Enums;

/// <summary>
/// Categorical properties that define card behavior through context rather than hardcoded values
/// </summary>

/// <summary>
/// Defines when a card can be played and how long it persists
/// </summary>
public enum PersistenceType
{
    /// <summary>
    /// Remains in hand until expressed - survives LISTEN actions
    /// </summary>
    Thought,

    /// <summary>
    /// Must be played immediately - removed after any SPEAK if unplayed
    /// </summary>
    Impulse,

    /// <summary>
    /// Time-limited opportunity - removed after LISTEN if unplayed
    /// </summary>
    Opening
}

/// <summary>
/// Type of effect when card play succeeds
/// </summary>
public enum SuccessEffectType
{
    /// <summary>
    /// No effect on success
    /// </summary>
    None,

    /// <summary>
    /// Changes rapport/connection (magnitude from difficulty)
    /// </summary>
    Rapport,

    /// <summary>
    /// Draws additional cards (magnitude from difficulty)
    /// </summary>
    Threading,

    /// <summary>
    /// Sets conversation atmosphere (specific type from magnitude from difficulty)
    /// </summary>
    Atmospheric,

    /// <summary>
    /// Restores focus points (magnitude from difficulty)
    /// </summary>
    Focusing,

    /// <summary>
    /// Moves obligation to position 1 and provides rapport
    /// </summary>
    Promising,

    /// <summary>
    /// Advances flow battery by magnitude from difficulty
    /// </summary>
    Advancing
}

/// <summary>
/// Type of penalty when card play fails
/// </summary>
public enum FailureEffectType
{
    /// <summary>
    /// No additional effect on failure (automatically applies ForceListen as fallback)
    /// </summary>
    None,

    /// <summary>
    /// Player must LISTEN on next turn after failure
    /// </summary>
    ForceListen,

    /// <summary>
    /// Negative rapport change (magnitude from difficulty)
    /// </summary>
    Backfire
}

/// <summary>
/// Effect when card is removed unplayed (exhausted/discarded)
/// </summary>
public enum ExhaustEffectType
{
    /// <summary>
    /// No effect when exhausted
    /// </summary>
    None,

    /// <summary>
    /// Draw cards when discarded (magnitude from difficulty)
    /// </summary>
    Threading,

    /// <summary>
    /// Restore focus when discarded (magnitude from difficulty)
    /// </summary>
    Focusing,

    /// <summary>
    /// Lose rapport when not played - the cost of silence (magnitude from difficulty)
    /// </summary>
    Regret
}