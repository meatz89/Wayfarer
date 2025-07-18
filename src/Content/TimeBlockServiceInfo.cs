/// <summary>
/// Information about services available during a specific time block
/// </summary>
public class TimeBlockServiceInfo
{
    public TimeBlocks TimeBlock { get; set; }
    public List<NPC> AvailableNPCs { get; set; } = new List<NPC>();
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public bool IsCurrentTimeBlock { get; set; }
}

/// <summary>
/// Service availability plan showing when a specific service is available
/// </summary>
public class ServiceAvailabilityPlan
{
    public ServiceTypes Service { get; set; }
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();
    public List<NPC> ServiceProviders { get; set; } = new List<NPC>();
}