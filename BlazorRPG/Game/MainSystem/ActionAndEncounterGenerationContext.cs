public class ActionAndEncounterGenerationContext
{
    public string LocationName { get; set; }
    public string LocationDescription { get; set; }
    public string SpotName { get; set; }
    public string SpotDescription { get; set; }
    public string InteractionType { get; set; }
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
}