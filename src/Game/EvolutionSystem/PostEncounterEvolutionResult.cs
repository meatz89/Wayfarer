
public class PostEncounterEvolutionResult
{
    public List<NewAction> NewActions { get; set; } = new List<NewAction>();
    public List<LocationSpot> NewLocationSpots { get; set; } = new List<LocationSpot>();
    public List<Location> NewLocations { get; set; } = new List<Location>();
    public List<NPC> NewCharacters { get; set; } = new List<NPC>();
    public ResourceChanges ResourceChanges { get; set; } = new ResourceChanges();
    public List<RelationshipChange> RelationshipChanges { get; set; } = new List<RelationshipChange>();
    public int CoinChange { get; set; } = 0;
    public List<object> InventoryChanges { get; set; }
}

public class RelationshipChange
{
    public string CharacterName { get; set; } = string.Empty;
    public int ChangeAmount { get; set; } = 0;
    public string Reason { get; set; } = string.Empty;
}

public class PlayerLocationUpdate
{
    public string NewLocationName { get; set; } = string.Empty;
    public bool LocationChanged { get; set; } = false;
}

public class ResourceChanges
{
    public int CoinChange { get; set; } = 0;
    public List<string> ItemsAdded { get; set; } = new List<string>();
    public List<string> ItemsRemoved { get; set; } = new List<string>();
}