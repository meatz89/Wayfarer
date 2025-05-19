public class Location
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<string> LocationSpotIds { get; set; } = new List<string>();
    public List<LocationSpot> LocationSpots { get; set; } = new List<LocationSpot>();

    public List<string> MorningProperties { get; set; } = new List<string>();
    public List<string> AfternoonProperties { get; set; } = new List<string>();
    public List<string> EveningProperties { get; set; } = new List<string>();
    public List<string> NightProperties { get; set; } = new List<string>();

    public Population? Population { get; set; } = Population.Quiet;
    public Atmosphere? Atmosphere { get; set; } = Atmosphere.Calm;
    public Physical? Physical { get; set; } = Physical.Confined;
    public Illumination? Illumination { get; set; } = Illumination.Bright;

    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public int Depth { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public int DiscoveryBonusXP { get; set; }
    public int DiscoveryBonusCoins { get; set; }
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public bool PlayerKnowledge { get; set; }

    public List<ILocationProperty> GetLocationProperties(TimeWindowTypes timeOfDay)
    {
        return new List<ILocationProperty>();
    }

    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
