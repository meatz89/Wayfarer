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
    /// Add fixed rapport value (positive or negative)
    /// </summary>
    AddRapport,
    
    /// <summary>
    /// Draw a number of cards
    /// </summary>
    DrawCards,
    
    /// <summary>
    /// Add presence to the presence pool
    /// </summary>
    AddPresence,
    
    /// <summary>
    /// Set the conversation atmosphere. Value indicates which atmosphere.
    /// </summary>
    SetAtmosphere,
    
    /// <summary>
    /// End the conversation. Data contains outcome details.
    /// </summary>
    EndConversation,
    
    /// <summary>
    /// Scale rapport by current flow level. Value contains formula like "4 - flow".
    /// </summary>
    ScaleRapportByFlow,
    
    /// <summary>
    /// Scale rapport by patience. Value contains formula like "patience / 3".
    /// </summary>
    ScaleRapportByPatience,
    
    /// <summary>
    /// Scale rapport by remaining presence. Value contains formula like "presence".
    /// </summary>
    ScaleRapportByPresence,
    
    /// <summary>
    /// Reset rapport to starting value
    /// </summary>
    RapportReset,
    
    /// <summary>
    /// Refresh presence to maximum
    /// </summary>
    PresenceRefresh,
    
    /// <summary>
    /// Next action costs 0 patience
    /// </summary>
    FreeNextAction,
    
    /// <summary>
    /// Exchange resources with NPC. Data contains cost and reward.
    /// </summary>
    Exchange
}
