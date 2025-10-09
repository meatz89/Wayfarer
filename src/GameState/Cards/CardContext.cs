public class CardContext
{
    public ExchangeCard ExchangeData { get; set; }
    public int MomentumThreshold { get; set; } // For goal cards
    public string RequestId { get; set; } // ID of the Goal this card belongs to
    public string GoalCardId { get; set; } // ID of the specific GoalCard (victory condition)

    // Additional context properties
    public ConnectionState ConnectionState { get; set; }
}
