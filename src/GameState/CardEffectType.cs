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
    /// Add fixed flow value (positive or negative)
    /// </summary>
    AddFlow,
    
    /// <summary>
    /// Draw a number of cards
    /// </summary>
    DrawCards,
    
    /// <summary>
    /// Add focus to the focus
    /// </summary>
    AddFocus,
    
    /// <summary>
    /// Set the conversation atmosphere. Value indicates which atmosphere.
    /// </summary>
    SetAtmosphere,
    
    /// <summary>
    /// End the conversation. Data contains outcome details.
    /// </summary>
    EndConversation,
    
    /// <summary>
    /// Scale flow by token count. Value contains token type.
    /// </summary>
    ScaleByTokens,
    
    /// <summary>
    /// Scale flow by current flow level. Value contains formula like "4 - flow".
    /// </summary>
    ScaleByFlow,
    
    /// <summary>
    /// Scale flow by patience. Value contains formula like "patience / 3".
    /// </summary>
    ScaleByPatience,
    
    /// <summary>
    /// Scale flow by remaining focus. Value contains formula like "focus".
    /// </summary>
    ScaleByFocus,
    
    /// <summary>
    /// Reset flow to 0
    /// </summary>
    FlowReset,
    
    /// <summary>
    /// Refresh focus to maximum
    /// </summary>
    FocusRefresh,
    
    /// <summary>
    /// Next action costs 0 patience
    /// </summary>
    FreeNextAction,
    
    /// <summary>
    /// Exchange resources with NPC. Data contains cost and reward.
    /// </summary>
    Exchange
}
