/// <summary>
/// Information about arbitrage opportunities
/// </summary>
public class MarketArbitrageInfo
{
public string ItemId { get; set; }
public string ItemName { get; set; }
public string BuyVenueId { get; set; }
public string BuyLocationName { get; set; }
public int BuyPrice { get; set; }
public string SellVenueId { get; set; }
public string SellLocationName { get; set; }
public int SellPrice { get; set; }
public int PotentialProfit { get; set; }
public MarketPriceInfo BestBuyLocation { get; set; }
public MarketPriceInfo BestSellLocation { get; set; }
public int MaxProfit { get; set; }
public List<MarketPriceInfo> AllPrices { get; set; }
}