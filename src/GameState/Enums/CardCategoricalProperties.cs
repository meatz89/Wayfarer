/// <summary>
/// Conversation card categories that define strategic role and mechanics
/// </summary>
public enum CardCategory
{
    /// <summary>
    /// Expression Cards: Generate momentum directly, represent statements that advance your position
    /// Effect types: Strike, Promising
    /// </summary>
    Expression,

    /// <summary>
    /// Realization Cards: Consume momentum for powerful effects, represent breakthrough moments
    /// Effect types: Advancing, DoubleMomentum
    /// </summary>
    Realization,

    /// <summary>
    /// Regulation Cards: Manage resources (focus, doubt, cards, flow), enable setup turns and recovery
    /// Effect types: Soothe, Threading, Focusing, Atmospheric
    /// </summary>
    Regulation
}

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
/// Type of effect when card play succeeds in deterministic momentum/doubt system
/// </summary>
public enum SuccessEffectType
{
    /// <summary>
    /// No effect on success
    /// </summary>
    None,

    /// <summary>
    /// Strike Effect: Gain momentum (uses scaling formulas)
    /// </summary>
    Strike,

    /// <summary>
    /// Soothe Effect: Reduce doubt (uses scaling formulas)
    /// </summary>
    Soothe,

    /// <summary>
    /// Thread Effect: Draw cards (uses scaling formulas)
    /// </summary>
    Threading,

    /// <summary>
    /// Double current momentum (powerful realization effect)
    /// </summary>
    DoubleMomentum,

    /// <summary>
    /// Sets conversation atmosphere (specific type from magnitude)
    /// </summary>
    Atmospheric,

    /// <summary>
    /// Restores focus points (magnitude from difficulty)
    /// </summary>
    Focusing,

    /// <summary>
    /// Moves obligation to position 1 and provides momentum
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

