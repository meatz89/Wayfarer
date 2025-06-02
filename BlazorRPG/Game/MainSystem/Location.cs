public class Location
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<string> LocationSpotIds { get; set; } = new List<string>();

    // Environmental properties by time window
    public List<string> MorningProperties { get; set; } = new List<string>();
    public List<string> AfternoonProperties { get; set; } = new List<string>();
    public List<string> EveningProperties { get; set; } = new List<string>();
    public List<string> NightProperties { get; set; } = new List<string>();

    // Tag Resonance System
    public List<string> DomainTags { get; set; } = new List<string>();

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
    public List<LocationSpot> AvailableSpots { get; set; } = new List<LocationSpot>();

    // Time-based properties
    public Dictionary<TimeOfDay, List<FlagStates>> TimeStateFlags { get; private set; }
    public Dictionary<TimeOfDay, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeOfDay, string> TimeSpecificDescription { get; private set; }
    public Dictionary<TimeOfDay, List<ILocationProperty>> TimeProperties { get; private set; }


    // Method to get current state based on time
    public List<FlagStates> GetCurrentFlags(TimeOfDay timeOfDay)
    {
        return TimeStateFlags.ContainsKey(timeOfDay)
            ? TimeStateFlags[timeOfDay]
            : new List<FlagStates>();
    }

    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
