public class LocationDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Difficulty { get; set; }
    public int Depth { get; set; }
    public string LocationType { get; set; }
    public List<string> AvailableServices { get; set; } = new List<string>();
    public int DiscoveryBonusXP { get; set; }
    public int DiscoveryBonusCoins { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
}
