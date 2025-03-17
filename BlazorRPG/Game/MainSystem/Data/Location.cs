public class Location
{
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
        bool playerKnowledge)
    {
        LocationName = locationName;
        TravelConnections = travelConnections;
        LocationSpots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
