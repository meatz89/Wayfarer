public class Location
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; set; }
    public List<LocationConnection> Connections { get; set; } = new List<LocationConnection>();
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
    // Discovery bonuses removed - new locations provide natural market opportunities instead
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public bool PlayerKnowledge { get; set; }
    public List<LocationSpot> AvailableSpots { get; set; } = new List<LocationSpot>();

    // Categorical Properties for NPC-Location Logical System Interactions
    public Dictionary<TimeBlocks, List<Professions>> AvailableProfessionsByTime { get; set; } = new Dictionary<TimeBlocks, List<Professions>>();

    // Time-based properties
    // Flag system removed - using connection tokens instead
    public Dictionary<TimeBlocks, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeBlocks, string> TimeSpecificDescription { get; private set; }
    public Dictionary<TimeBlocks, List<ILocationProperty>> TimeProperties { get; private set; }
    public List<string> ConnectedLocationIds { get; internal set; }
    public List<Item> MarketItems { get; internal set; }
    public List<RestOption> RestOptions { get; internal set; }
    
    // Access Requirements for this location
    public AccessRequirement AccessRequirement { get; set; }


    // Flag system removed - using connection tokens instead

    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public bool IsProfessionAvailable(Professions profession, TimeBlocks currentTime)
    {
        if (!AvailableProfessionsByTime.ContainsKey(currentTime))
            return false;

        return AvailableProfessionsByTime[currentTime].Contains(profession);
    }

    public List<Professions> GetAvailableProfessions(TimeBlocks currentTime)
    {
        return AvailableProfessionsByTime.ContainsKey(currentTime)
            ? AvailableProfessionsByTime[currentTime]
            : new List<Professions>();
    }

}
