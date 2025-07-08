public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Weight { get; set; } = 1;
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public int InventorySlots { get; set; } = 1;
    public List<string> EnabledRouteTypes { get; set; } = new List<string>();
    public bool IsContraband { get; set; } = false;
    public string LocationId { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }

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

    public bool IsAvailable { get; internal set; }
}