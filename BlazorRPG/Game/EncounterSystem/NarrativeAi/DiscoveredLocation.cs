// General world information for discovery context
// Context for developing specific entities
// Minimal location information from discovery
public class DiscoveredLocation
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedLocations { get; set; }
}
