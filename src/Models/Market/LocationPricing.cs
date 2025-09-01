/// <summary>
/// Represents pricing information for an item at a specific location
/// </summary>
public class LocationPricing
{
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public float SupplyLevel { get; set; } // 0.5 = scarce, 1.0 = normal, 2.0 = abundant
    public bool IsAvailable { get; set; }
}