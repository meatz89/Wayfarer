/// <summary>
/// Defines how a card affects Cadence when played during SPEAK action
/// Delivery is NOT a card effect - it's a game rule that determines cadence change
/// </summary>
public enum DeliveryType
{
    /// <summary>
    /// Standard delivery: +1 Cadence (most cards)
    /// </summary>
    Standard,

    /// <summary>
    /// Commanding delivery: +2 Cadence (ALL Authority cards, pushes conversation forward)
    /// </summary>
    Commanding,

    /// <summary>
    /// Measured delivery: +0 Cadence (careful, thoughtful statements)
    /// Used by Rapport questions, Diplomacy risk management, some Insight/Cunning
    /// </summary>
    Measured,

    /// <summary>
    /// Yielding delivery: -1 Cadence (deferential, letting NPC lead)
    /// Rare, used by some Rapport cards (gentle encouragement, listening signals)
    /// </summary>
    Yielding
}
