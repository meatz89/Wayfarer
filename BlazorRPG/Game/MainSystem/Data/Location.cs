public class Location
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }

    // Physical connections
    public List<string> ConnectedLocationIds { get; set; } = new List<string>();
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }

    // Interaction spots within the location
    public List<LocationSpot> Spots { get; set; } = new List<LocationSpot>();

    // Mechanical properties
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; } = new List<IEnvironmentalProperty>();
    public Dictionary<string, List<IEnvironmentalProperty>> TimeProperties { get; set; } = new Dictionary<string, List<IEnvironmentalProperty>>();
    public int Difficulty { get; set; }

    // Narrative elements (directly from AI)
    public string DetailedDescription { get; set; }
    public string History { get; set; }
    public string PointsOfInterest { get; set; }

    // Strategic gameplay elements
    public List<StrategicTag> StrategicTags { get; set; } = new List<StrategicTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();

    public bool PlayerKnowledge { get; }

    public Location()
    {

    }

    public Location(
        string locationName,
        List<string> travelConnections,
        List<LocationSpot> locationSpots,
        int difficultyLevel,
        bool playerKnowledge)
    {
        Name = locationName.ToString();
        ConnectedLocationIds = travelConnections;
        Spots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
