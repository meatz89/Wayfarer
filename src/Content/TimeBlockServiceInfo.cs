public class TimeBlockServiceInfo
{
    public TimeBlocks TimeBlock { get; set; }
    public List<NPC> AvailableNPCs { get; set; } = new List<NPC>();
    public List<string> AvailableServices { get; set; } = new List<string>();
    public bool IsCurrentTimeBlock { get; set; }
}

public class ServiceAvailabilityPlan
{
    public string Service { get; set; }
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();
    public List<NPC> ServiceProviders { get; set; } = new List<NPC>();
}
