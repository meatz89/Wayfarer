/// <summary>
/// Represents an item available in the market with trader-specific information
/// </summary>
public class MarketItem
{
    // HIGHLANDER: NO Id property - MarketItem identified by object reference
    public string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    // HIGHLANDER: Object reference ONLY, no TraderId
    public NPC Trader { get; set; }
    // Item property already exists below, no need for ItemId

    public int BuyPrice => Price;
    public int SellPrice => Math.Max(1, Price - 3); // DDR-007: Flat 3-coin spread
    public List<ItemCategory> Categories { get; set; } = new List<ItemCategory>();
    public Item Item { get; set; } // Reference to actual item
}