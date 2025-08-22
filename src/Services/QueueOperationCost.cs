public class QueueOperationCost
{
    public Dictionary<ConnectionType, int> TokenCosts { get; set; } = new Dictionary<ConnectionType, int>();
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public bool IsAffordable { get; set; }
}
