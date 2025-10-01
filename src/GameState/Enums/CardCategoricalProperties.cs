/// <summary>
/// Card Depth determines stat requirements and strategic tier (1-10 system)
/// </summary>
public enum CardDepth
{
    Depth1 = 1, Depth2 = 2, Depth3 = 3, // Foundation Cards (0-2 Initiative)
    Depth4 = 4, Depth5 = 5, Depth6 = 6, // Standard Cards (3-5 Initiative)
    Depth7 = 7, Depth8 = 8, Depth9 = 9, Depth10 = 10 // Decisive Cards (6-12 Initiative)
}

/// <summary>
/// Defines card persistence behavior - ONLY 2 types exist
/// </summary>
public enum PersistenceType
{
    /// <summary>
    /// Statement: One-time content that stays permanently in Spoken pile after playing, NEVER reshuffles
    /// </summary>
    Statement,

    /// <summary>
    /// Echo: Repeatable techniques that can reshuffle from Spoken pile back to Deck for reuse
    /// </summary>
    Echo
}

/// <summary>
/// Type of effect when card play succeeds in the new 4-resource system
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
    /// Moves obligation to position 1 and provides momentum
    /// </summary>
    Promising,

}


/// <summary>
/// Conversational moves - natural categories of what you say.
/// Remark = Simple pointed statement (Momentum building)
/// Observation = Simple supportive comment (questions, understanding, reassurance)
/// Argument = Complex developed point (requires conversational buildup)
/// Determines Initiative generation behavior categorically.
/// </summary>
public enum ConversationalMove
{
    /// <summary>
    /// Remark: Simple pointed statement that presses the conversation forward (generates Initiative)
    /// </summary>
    Remark,

    /// <summary>
    /// Observation: Simple supportive comment - asking, noticing, understanding, reassuring (generates Initiative)
    /// </summary>
    Observation,

    /// <summary>
    /// Argument: Complex developed point requiring conversational buildup (costs Initiative, no generation)
    /// </summary>
    Argument
}

/// <summary>
/// Scaling formula for card effects based on visible game state
/// </summary>
public class ScalingFormula
{
    public string ScalingType { get; set; } // "Cadence", "Doubt", "SpokeCards", "Momentum"
    public int BaseEffect { get; set; }
    public decimal Multiplier { get; set; }
    public string Formula { get; set; } // Human-readable formula description
}

