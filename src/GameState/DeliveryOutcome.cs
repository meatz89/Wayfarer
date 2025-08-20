public class DeliveryOutcome
{
    public int BasePayment { get; set; }
    public int BonusPayment { get; set; }
    public ConnectionType TokenType { get; set; }
    public int TokenAmount { get; set; }
    public bool TokenReward { get; set; } = true;
    public bool TokenPenalty { get; set; } = false;
    public bool ReducesLeverage { get; set; } = false;
    public string AdditionalEffect { get; set; }
}