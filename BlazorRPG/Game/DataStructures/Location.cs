public class Location
{
    public LocationNames Name { get; set; }
    public LocationTypes CoreType;
    public TimeWindows TimeWindows { get; set; }
    public List<LocationNames> ConnectedLocations = new();
    public List<LocationSpot> Spots = new();

    public List<BasicAction> CoreActions = new();
    public bool HasBadShelter = false;
    public bool HasShelter = false;
    public int ShelterCost = 1;
}