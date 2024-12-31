public class Location
{
    public LocationNames Name { get; set; }
    public LocationTypes CoreType;
    public TimeWindows TimeWindows { get; set; }
    public List<LocationNames> ConnectedLocations { get; set; }

    public List<BasicAction> AvailableActions = new();
    public bool HasBadShelter = false;
    public bool HasShelter = false;
    public int ShelterCost = 1;
}