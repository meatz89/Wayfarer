
// Resource exchange for trade
public class ResourceExchange
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<ResourceType, int> PlayerGives { get; set; } = new();
    public Dictionary<ResourceType, int> PlayerReceives { get; set; } = new();
    public int RequiredTrust { get; set; }
    public bool SingleUse { get; set; }
    
    // Additional properties used in CardDeckLoader
    public ResourceType ResourceType { get; set; }
    public int Amount { get; set; }
    public bool IsAbsolute { get; set; }
}
