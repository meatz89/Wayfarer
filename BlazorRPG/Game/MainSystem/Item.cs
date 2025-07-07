public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Weight { get; set; } = 1;  // Default weight of 1
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int InventorySlots { get; set; } = 1;  // For bulk items
    public List<string> EnabledRouteTypes { get; set; } = new List<string>();
    
    public string WeightDescription => Weight switch
    {
        0 => "Weightless",
        1 => "Light",
        2 => "Medium",
        3 => "Heavy",
        _ => $"Very Heavy ({Weight})"
    };

    // Helper property for UI display
    public string RouteTypesDescription => EnabledRouteTypes.Any()
        ? $"Enables: {string.Join(", ", EnabledRouteTypes)}"
        : "";
}