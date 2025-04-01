public class WorldEvolutionResponse
{
    public List<LocationSpot> NewLocationSpots { get; set; } = new List<LocationSpot>();
    public List<NewAction> NewActions { get; set; } = new List<NewAction>();
    public List<Character> NewCharacters { get; set; } = new List<Character>();
    public List<Location> NewLocations { get; set; } = new List<Location>();
    public List<Opportunity> NewOpportunities { get; set; } = new List<Opportunity>();
    public int CoinChange { get; set; } = 0;

    public PlayerLocationUpdate LocationUpdate { get; set; } = new PlayerLocationUpdate();
    public ResourceChanges ResourceChanges { get; set; } = new ResourceChanges();
}

public class PlayerLocationUpdate
{
    public string NewLocationName { get; set; } = string.Empty;
    public bool LocationChanged { get; set; } = false;
}

public class ResourceChanges
{
    public List<string> ItemsAdded { get; set; } = new List<string>();
    public List<string> ItemsRemoved { get; set; } = new List<string>();
}
