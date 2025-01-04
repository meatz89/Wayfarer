public class Location
{
    public LocationNames Name { get; set; }
    public LocationTypes CoreType;
    public TimeSlots TimeWindows { get; set; }
    public List<LocationNames> ConnectedLocations = new();
    public List<LocationSpot> Spots = new();
}