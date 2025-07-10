public enum Social_Expectation
{
    Any,
    Merchant_Class,
    Noble_Class,
    Professional
}

public enum Access_Level
{
    Public,
    Semi_Private,
    Private,
    Restricted
}

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
    public Social_Expectation SocialExpectation { get; set; } = Social_Expectation.Any;
    public Access_Level AccessLevel { get; set; } = Access_Level.Public;
    public List<Social_Class> RequiredSocialClasses { get; set; } = new List<Social_Class>();
    public Dictionary<TimeBlocks, List<Professions>> AvailableProfessionsByTime { get; set; } = new Dictionary<TimeBlocks, List<Professions>>();

    // Time-based properties
    public Dictionary<TimeBlocks, List<FlagStates>> TimeStateFlags { get; private set; }
    public Dictionary<TimeBlocks, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeBlocks, string> TimeSpecificDescription { get; private set; }
    public Dictionary<TimeBlocks, List<ILocationProperty>> TimeProperties { get; private set; }
    public List<string> ConnectedLocationIds { get; internal set; }
    public List<Item> MarketItems { get; internal set; }
    public List<RestOption> RestOptions { get; internal set; }


    // Method to get current state based on time
    public List<FlagStates> GetCurrentFlags(TimeBlocks TimeBlocks)
    {
        return TimeStateFlags.ContainsKey(TimeBlocks)
            ? TimeStateFlags[TimeBlocks]
            : new List<FlagStates>();
    }

    public Location(string id, string name)
    {
        Id = id;
        Name = name;
    }
    
    // Helper methods for categorical system interactions
    public bool CanNPCAccessLocation(NPC npc)
    {
        // Check social expectations
        if (!npc.MeetsLocationRequirements(SocialExpectation))
            return false;
            
        // Check specific social class requirements
        if (RequiredSocialClasses.Any() && !RequiredSocialClasses.Contains(npc.SocialClass))
            return false;
            
        return true;
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
    
    // Helper properties for UI display
    public string SocialExpectationDescription => SocialExpectation.ToString().Replace('_', ' ');
    public string AccessLevelDescription => AccessLevel.ToString().Replace('_', ' ');
    
    public string RequiredSocialClassesDescription => RequiredSocialClasses.Any()
        ? $"Required: {string.Join(", ", RequiredSocialClasses.Select(c => c.ToString().Replace('_', ' ')))}"
        : "No social restrictions";
}
