public class Location
{
    public Location(
        LocationTypes locationType,
        LocationNames locationName,
        LocationArchetypes locationArchetype,
        List<LocationNames> travelResonances,
        List<LocationSpot> locationSpots,
        LocationProperties locationProperties,
        int difficultyLevel)
    {
        LocationType = locationType;
        LocationName = locationName;
        Archetype = locationArchetype;
        TravelConnections = travelResonances;
        LocationSpots = locationSpots;
        LocationProperties = locationProperties;
        DifficultyLevel = difficultyLevel;
    }

    public LocationTypes LocationType { get; set; } // Industrial/Commercial/etc
    public LocationNames LocationName { get; set; }
    public LocationArchetypes Archetype { get; set; } // Tavern/Market/Dock/etc
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings
    public LocationProperties LocationProperties { get; set; }

    public int DifficultyLevel { get; set; }

}
