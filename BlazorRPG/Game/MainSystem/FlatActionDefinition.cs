public class FlatActionDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string LocationSpotId { get; set; } = "";
    public EncounterDefinition EncounterDefinition { get; set; }
}
