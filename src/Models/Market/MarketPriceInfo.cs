/// <summary>
/// Market price information for an item at a location
/// HIGHLANDER: Object references, no string IDs
/// TYPE SYSTEM: int only for game values (no float/double)
/// </summary>
public class MarketPriceInfo
{
    public Venue Venue { get; set; }
    public string LocationName { get; set; }
    public Item Item { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int SupplyLevel { get; set; } // 0-100 percentage
    public bool IsCurrentLocation { get; set; }
    public bool CanBuy { get; set; }
}