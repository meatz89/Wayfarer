/// <summary>
/// Market price information for an item at a location
/// </summary>
public class MarketPriceInfo
{
    public string LocationId { get; set; }
    public string LocationName { get; set; }
    public string ItemId { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public float SupplyLevel { get; set; }
    public bool IsCurrentLocation { get; set; }
    public bool CanBuy { get; set; }
}