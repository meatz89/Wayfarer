public class Location
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; } = new List<IEnvironmentalProperty>();

    // Interaction spots within the location
    public List<LocationSpot> Spots { get; set; } = new List<LocationSpot>();

    public void AddSpot(LocationSpot spot)
    {
        Spots.Add(spot);
    }

    // Physical connections
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }


    // Mechanical properties
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
        ConnectedTo = travelConnections;
        Spots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
