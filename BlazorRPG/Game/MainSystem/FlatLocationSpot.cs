public class FlatLocationSpot
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string InteractionType { get; set; } = "";
    public string Position { get; set; } = "";
    public string LocationName { get; set; } = "";
    public Dictionary<string, string> EnvironmentalProperties { get; set; } = new Dictionary<string, string>();
}
