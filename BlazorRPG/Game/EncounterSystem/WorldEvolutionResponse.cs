public class WorldEvolutionResponse
{
    public List<LocationSpot> NewLocationSpots { get; set; } = new List<LocationSpot>();
    public List<NewAction> NewActions { get; set; } = new List<NewAction>();
    public List<Character> NewCharacters { get; set; } = new List<Character>();
    public List<Location> NewLocations { get; set; } = new List<Location>();
    public List<Opportunity> NewOpportunities { get; set; } = new List<Opportunity>();
}
