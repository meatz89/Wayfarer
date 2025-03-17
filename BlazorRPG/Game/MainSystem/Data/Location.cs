public class Location
{
    public string Name { get; set; }
    public int Duration { get; set; }
    public int FailThreshold { get; set; }
    public int PartialSuccessThreshold { get; set; }
    public int StandardSuccessThreshold { get; set; }
    public int ExceptionalSuccessThreshold { get; set; }

    public LocationNames LocationName { get; set; }
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings
    public int Difficulty { get; set; }
    public ItemTypes ItemType { get; }
    public bool PlayerKnowledge { get; }

    public Location(
        LocationNames locationName,
        List<LocationNames> travelConnections,
        List<LocationSpot> locationSpots,
        int difficultyLevel,
        bool playerKnowledge,
        string name = "None",
        int duration = 5,
        int failThreshold = 7,
        int partialSuccessThreshold = 8,
        int standardSuccessThreshold = 10,
        int exceptionalSuccessThreshold = 12)
    {
        Name = name;
        Duration = duration;
        FailThreshold = failThreshold;
        PartialSuccessThreshold = partialSuccessThreshold;
        StandardSuccessThreshold = standardSuccessThreshold;
        ExceptionalSuccessThreshold = exceptionalSuccessThreshold;

        LocationName = locationName;
        TravelConnections = travelConnections;
        LocationSpots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
