public class Location
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public string DetailedDescription { get; set; }
    public string History { get; set; }
    public string PointsOfInterest { get; set; }
    public List<StrategicTag> StrategicTags { get; set; } = new List<StrategicTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();
    public int Depth { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public int DiscoveryBonusXP { get; set; }
    public int DiscoveryBonusCoins { get; set; }
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public bool PlayerKnowledge { get; set; }

    public Location(string id)
    {
        Id = id;
    }
}
