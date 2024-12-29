public class Location
{
    public LocationNames Name { get; set; }
    public string Description { get; set; }
    public List<LocationNames> ConnectedLocations { get; set; }
}
