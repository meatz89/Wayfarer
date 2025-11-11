/// <summary>
/// Types of social/interpersonal events that can clear player states
/// Used by StateClearingBehavior for event-based state clearing
/// </summary>
public enum SocialEventType
{
    /// <summary>
    /// Receive comfort from an NPC - clears distressed/depressed states
    /// </summary>
    ReceiveComfort,

    /// <summary>
    /// Betray a trust relationship - clears Trusted status, may apply new states
    /// </summary>
    BetrayTrust,

    /// <summary>
    /// Remove a disguise - clears Disguised state
    /// </summary>
    RemoveDisguise,

    /// <summary>
    /// Identity revealed through events - clears Incognito/Hidden states
    /// </summary>
    IdentityRevealed
}
