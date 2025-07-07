public class Item
{
    public string Name { get; set; }
    public int Weight { get; set; }  // 0-3 scale
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int InventorySlots { get; set; } = 1;  // For bulk items that take multiple slots
    public List<string> EnabledRouteTypes { get; set; } = new List<string>();
}
