public class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; } = new List<IEnvironmentalProperty>();
    public List<LocationSpot> LocationSpots { get; set; } = new List<LocationSpot>();
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public string DetailedDescription { get; set; }
    public string History { get; set; }
    public string PointsOfInterest { get; set; }
    public List<StrategicTag> StrategicTags { get; set; } = new List<StrategicTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();
    public bool PlayerKnowledge { get; }

    public int Depth { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public int DiscoveryBonusXP { get; set; }
    public int DiscoveryBonusCoins { get; set; }
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }

    public void AddSpot(LocationSpot spot)
    {
        LocationSpots.Add(spot);
    }

    public Location()
    {

    }

    public Location(
        string locationName,
        string description,
        List<string> travelConnections,
        List<LocationSpot> locationSpots,
        int difficultyLevel,
        bool playerKnowledge)
    {
        Name = locationName.ToString();
        Description = description;
        ConnectedTo = travelConnections;
        LocationSpots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
