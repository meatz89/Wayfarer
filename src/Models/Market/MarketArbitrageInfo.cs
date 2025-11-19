/// <summary>
/// Information about arbitrage opportunities
/// HIGHLANDER: Object references, no string IDs
/// </summary>
public class MarketArbitrageInfo
{
    public Item Item { get; set; }
    public string ItemName { get; set; }
    public Venue BuyVenue { get; set; }
    public string BuyLocationName { get; set; }
    public int BuyPrice { get; set; }
    public Venue SellVenue { get; set; }
    public string SellLocationName { get; set; }
    public int SellPrice { get; set; }
    public int PotentialProfit { get; set; }
    public MarketPriceInfo BestBuyLocation { get; set; }
    public MarketPriceInfo BestSellLocation { get; set; }
    public int MaxProfit { get; set; }
    public List<MarketPriceInfo> AllPrices { get; set; }
}