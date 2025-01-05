public class Location
{
    public LocationTypes LocationType { get; set; }
    public LocationNames Name { get; set; }
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; }
}