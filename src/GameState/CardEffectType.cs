/// <summary>
/// Types of effects that cards can trigger
/// </summary>
public enum CardEffectType
{
    /// <summary>
    /// No effect
    /// </summary>
    None,
    
    /// <summary>
    /// Add fixed comfort value (positive or negative)
    /// </summary>
    AddComfort,
    
    /// <summary>
    /// Draw a number of cards
    /// </summary>
    DrawCards,
    
    /// <summary>
    /// Add weight to the weight pool
    /// </summary>
    AddWeight,
    
    /// <summary>
    /// Set the conversation atmosphere. Value indicates which atmosphere.
    /// </summary>
    SetAtmosphere,
    
    /// <summary>
    /// End the conversation. Data contains outcome details.
    /// </summary>
    EndConversation,
    
    /// <summary>
    /// Scale comfort by token count. Value contains token type.
    /// </summary>
    ScaleByTokens,
    
    /// <summary>
    /// Scale comfort by current comfort level. Value contains formula like "4 - comfort".
    /// </summary>
    ScaleByComfort,
    
    /// <summary>
    /// Scale comfort by patience. Value contains formula like "patience / 3".
    /// </summary>
    ScaleByPatience,
    
    /// <summary>
    /// Scale comfort by remaining weight. Value contains formula like "weight".
    /// </summary>
    ScaleByWeight,
    
    /// <summary>
    /// Reset comfort to 0
    /// </summary>
    ComfortReset,
    
    /// <summary>
    /// Refresh weight pool to maximum
    /// </summary>
    WeightRefresh,
    
    /// <summary>
    /// Next action costs 0 patience
    /// </summary>
    FreeNextAction,
    
    // Legacy compatibility (will be removed)
    FixedComfort,      // Maps to AddComfort
    ScaledComfort,     // Maps to ScaleByTokens/ScaleByComfort/etc
    ObservationEffect, // Maps to specific observation effects
    GoalEffect,        // Maps to EndConversation
}
