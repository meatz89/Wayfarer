public class FlatPostEncounterEvolutionResponse
{
    public ResourceChanges ResourceChanges { get; set; }
    public List<RelationshipChange> RelationshipChanges { get; set; } = new List<RelationshipChange>();
    public List<LocationDefinition> Locations { get; set; } = new List<LocationDefinition>();
    public List<LocationSpotDefinition> LocationSpots { get; set; } = new List<LocationSpotDefinition>();
    public List<EvolutionActionTemplate> ActionDefinitions { get; set; } = new List<EvolutionActionTemplate>();
    public List<NPC> Characters { get; set; } = new List<NPC>();
    public List<Contract> Contracts { get; set; } = new List<Contract>();
}
