public class DayCycleDefinition
{
    public string RegionId { get; set; }
    public List<TimeStateDefinition> TimeStates { get; set; } = new List<TimeStateDefinition>();
    public List<ScheduledEvent> ScheduledEvents { get; set; } = new List<ScheduledEvent>();
    public List<NarrativeTrigger> NarrativeTriggers { get; set; } = new List<NarrativeTrigger>();
}
