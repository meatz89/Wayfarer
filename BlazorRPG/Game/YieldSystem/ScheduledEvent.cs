public class ScheduledEvent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public TimeRange TimeRange { get; set; }
    public string LocationId { get; set; }
    public List<YieldDefinition> Yields { get; set; } = new List<YieldDefinition>();
    public List<string> RequiredConditions { get; set; } = new List<string>();
}
