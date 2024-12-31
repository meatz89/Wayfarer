public class Location
{
    public LocationNames LocationName { get; set; }
    public List<LocationNames> ConnectedLocations { get; set; }
    public LocationTypes LocationType;

    public List<BasicAction> Actions = new();
}