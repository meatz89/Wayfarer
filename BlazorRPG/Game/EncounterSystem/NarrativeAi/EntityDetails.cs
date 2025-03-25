// General world information for discovery context
// Context for developing specific entities
// Minimal location information from discovery
// Detailed entity information from development
public class EntityDetails
{
    public string DetailedDescription { get; set; }
    public string Background { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; }
    public Dictionary<string, List<IEnvironmentalProperty>> TimeBasedProperties { get; set; }
}
