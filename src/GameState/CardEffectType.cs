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
    /// Add focus to the focus pool
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
    /// Scale rapport by current flow level. Value contains formula like "4 - flow".
    /// </summary>
    ScaleRapportByFlow,

    /// <summary>
    /// Scale rapport by patience. Value contains formula like "patience / 3".
    /// </summary>
    ScaleRapportByPatience,

    /// <summary>
    /// Scale rapport by remaining focus. Value contains formula like "focus".
    /// </summary>
    ScaleRapportByFocus,

    /// <summary>
    /// Reset rapport to starting value
    /// </summary>
    RapportReset,

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
    Exchange,

    /// <summary>
    /// Advance NPC to specified connection state (observation cards only)
    /// </summary>
    AdvanceConnectionState,

    /// <summary>
    /// Unlock a hidden exchange option (observation cards only)  
    /// </summary>
    UnlockExchange,

    /// <summary>
    /// Offer a letter for delivery (letter cards only)
    /// </summary>
    OfferLetter,

    /// <summary>
    /// Gain connection tokens (goal cards)
    /// </summary>
    GainToken,

    /// <summary>
    /// Force an obligation to queue position 1, burning tokens with displaced NPCs.
    /// Promise cards use this to manipulate queue at the cost of relationships.
    /// Value contains the position to force (usually "1").
    /// </summary>
    ForceQueuePosition
}
