/// <summary>
/// Represents an NPC's schedule entry for a specific time block.
/// Used by the INVESTIGATE verb to reveal NPC availability patterns.
/// </summary>
public class ScheduleEntry
{
    public TimeBlocks TimeBlock { get; set; }
    public string LocationId { get; set; }
    public string Activity { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    public ScheduleEntry() { }
    
    public ScheduleEntry(TimeBlocks timeBlock, string locationId, string activity = "Available", bool isAvailable = true)
    {
        TimeBlock = timeBlock;
        LocationId = locationId;
        Activity = activity;
        IsAvailable = isAvailable;
    }
}