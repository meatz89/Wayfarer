/// <summary>
/// Market price information for an item at a location
/// HIGHLANDER: Object references, no string IDs
/// </summary>
public class MarketPriceInfo
{
    public Venue Venue { get; set; }
    public string LocationName { get; set; }
    public Item Item { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public float SupplyLevel { get; set; }
    public bool IsCurrentLocation { get; set; }
    public bool CanBuy { get; set; }
}