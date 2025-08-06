/// <summary>
/// Represents an item available in the market with trader-specific information
/// </summary>
public class MarketItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public string TraderId { get; set; }
    public string ItemId { get; set; }

    // Additional properties for compatibility
    public int BuyPrice => Price;
    public int SellPrice => (int)(Price * 0.7); // 70% of buy price
    public List<ItemCategory> Categories { get; set; } = new List<ItemCategory>();
    public Item Item { get; set; } // Reference to actual item
}