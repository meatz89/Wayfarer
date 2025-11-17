public class CardContext
{
    public ExchangeCard ExchangeData { get; set; }

    /// <summary>
    /// Universal threshold for situation cards
    /// Interpretation depends on system type:
    /// - Social: Momentum threshold
    /// - Mental: Progress threshold
    /// - Physical: Breakthrough threshold
    /// </summary>
    public int threshold { get; set; }

    public Situation Situation { get; set; } // Object reference to the Situation this card belongs to
    // ADR-007: SituationCardId DELETED - unused dead code (never read)

    // Additional context properties
    public ConnectionState ConnectionState { get; set; }
}
