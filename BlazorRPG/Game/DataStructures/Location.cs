public class Location
{
    public LocationTypes LocationType { get; set; } // Industrial/Commercial/etc
    public LocationNames LocationName { get; set; }
    public LocationArchetype Archetype { get; set; } // Tavern/Market/Dock/etc
    public LocationProperties Properties { get; set; } // Properties like Indoor/Crowded/etc
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings

}
