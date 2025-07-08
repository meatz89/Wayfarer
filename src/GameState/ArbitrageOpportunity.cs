/// <summary>
/// Represents a profitable trading opportunity between two locations.
/// Used for calculating and displaying arbitrage opportunities to players.
/// </summary>
public class ArbitrageOpportunity
{
    public string ItemId { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int Profit { get; set; }
    public float ProfitMargin { get; set; }
}