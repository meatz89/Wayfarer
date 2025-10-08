public class CardContext
{
    public ExchangeCard ExchangeData { get; set; }
    public int MomentumThreshold { get; set; } // For request/promise cards
    public string RequestId { get; set; } // ID of the NPCRequest this card belongs to

    // Additional context properties  
    public ConnectionState ConnectionState { get; set; }
}
