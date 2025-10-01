/// <summary>
/// Categorical reasons for letter queue positioning.
/// Backend sets these categories, frontend translates to user-facing text.
/// </summary>
public enum LetterPositioningReason
{
    /// <summary>
    /// DeliveryObligation positioned due to active standing obligation with sender
    /// Priority: Highest (position 1)
    /// </summary>
    Obligation,

    /// <summary>
    /// DeliveryObligation positioned due to Diplomacy debt >= 3 tokens
    /// Priority: Very High (position 2)
    /// </summary>
    DiplomacyDebt,

    /// <summary>
    /// DeliveryObligation positioned due to negative token balance (debt)
    /// Priority: Low (pushed down by debt penalty)
    /// </summary>
    PoorStanding,

    /// <summary>
    /// DeliveryObligation positioned due to positive token balance 
    /// Priority: High (moved up by relationship strength)
    /// </summary>
    GoodStanding,

    /// <summary>
    /// DeliveryObligation positioned at default location (no significant relationship)
    /// Priority: Normal (position 8 base)
    /// </summary>
    Neutral
}