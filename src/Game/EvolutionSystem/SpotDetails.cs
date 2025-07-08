public class SpotDetails
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string InteractionDescription { get; set; } = "";
    public Dictionary<string, string> EnvironmentalProperties { get; set; } = new Dictionary<string, string>();
}