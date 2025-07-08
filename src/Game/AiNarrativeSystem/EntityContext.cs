// General world information for discovery context
// Context for developing specific entities
public class EntityContext
{
    public string Name { get; set; }
    public string CurrentDescription { get; set; }
    public Dictionary<string, string> RelatedEntities { get; set; }
    public List<string> InteractionHistory { get; set; }
}
