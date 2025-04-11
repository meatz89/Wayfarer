public class SpotDetails
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string InteractionType { get; set; } = "";
    public string InteractionDescription { get; set; } = "";
    public string Position { get; set; } = "";
    public List<string> ActionNames { get; set; } = new List<string>();
    public Dictionary<string, string> EnvironmentalProperties { get; set; } = new Dictionary<string, string>();
}