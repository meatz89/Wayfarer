/// <summary>
/// Represents a profitable trading opening between two locations.
/// Used for calculating and displaying arbitrage opportunities to players.
/// </summary>
public class ArbitrageOpening
{
public string ItemId { get; set; }
public int BuyPrice { get; set; }
public int SellPrice { get; set; }
public int Profit { get; set; }
public float ProfitMargin { get; set; }
}