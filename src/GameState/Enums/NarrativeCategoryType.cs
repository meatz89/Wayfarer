/// <summary>
/// Narrative category for backwards construction in social challenges.
/// Determines what kind of NPC dialogue the card should respond to.
/// Derived from card's mechanical properties at parse-time.
/// </summary>
public enum NarrativeCategoryType
{
    /// <summary>
    /// Standard conversational response - default category
    /// </summary>
    Standard,

    /// <summary>
    /// Atmosphere-changing cards that shift conversation tone
    /// Cards with no success effect (SuccessType.None)
    /// </summary>
    Atmosphere,

    /// <summary>
    /// Pressure cards that press for information or commitment
    /// Statement cards (PersistenceType.Statement)
    /// </summary>
    Pressure,

    /// <summary>
    /// Support cards that build connection through Trust tokens
    /// TokenType = ConnectionType.Trust
    /// </summary>
    SupportTrust,

    /// <summary>
    /// Support cards that build connection through Diplomacy tokens
    /// TokenType = ConnectionType.Diplomacy
    /// </summary>
    SupportDiplomacy,

    /// <summary>
    /// Support cards that build connection through Status tokens
    /// TokenType = ConnectionType.Status
    /// </summary>
    SupportStatus,

    /// <summary>
    /// Support cards that build connection through Shadow tokens
    /// TokenType = ConnectionType.Shadow
    /// </summary>
    SupportShadow
}
