/// <summary>
/// Active State instance - temporary condition currently affecting the player
/// Stored in Player.ActiveStates list, tracked with segment-based duration
/// </summary>
public class ActiveState
{
/// <summary>
/// Type of state (from StateType enum)
/// </summary>
public StateType Type { get; set; }

/// <summary>
/// Category of this state (Physical, Mental, Social)
/// </summary>
public StateCategory Category { get; set; }

/// <summary>
/// Day when state was applied
/// </summary>
public int AppliedDay { get; set; }

/// <summary>
/// Time block when state was applied
/// </summary>
public TimeBlocks AppliedTimeBlock { get; set; }

/// <summary>
/// Segment within time block when state was applied (1-4)
/// </summary>
public int AppliedSegment { get; set; }

/// <summary>
/// Duration in segments before state auto-clears
/// null = does not auto-clear based on time, must be cleared manually or by condition
/// </summary>
public int? DurationSegments { get; set; }

/// <summary>
/// Calculate absolute segment when state was applied
/// </summary>
public int GetAbsoluteAppliedSegment()
{
    int blockOffset = AppliedTimeBlock switch
    {
        TimeBlocks.Morning => 0,
        TimeBlocks.Midday => 4,
        TimeBlocks.Afternoon => 8,
        TimeBlocks.Evening => 12,
        _ => 0
    };
    return blockOffset + AppliedSegment;
}

/// <summary>
/// Check if state should auto-clear based on current time
/// </summary>
public bool ShouldAutoClear(int currentDay, TimeBlocks currentTimeBlock, int currentSegment)
{
    if (!DurationSegments.HasValue)
        return false; // Manual clear only

    int currentAbsoluteSegment = currentDay * 16 + currentTimeBlock switch
    {
        TimeBlocks.Morning => 0,
        TimeBlocks.Midday => 4,
        TimeBlocks.Afternoon => 8,
        TimeBlocks.Evening => 12,
        _ => 0
    } + currentSegment;

    int appliedAbsoluteSegment = AppliedDay * 16 + GetAbsoluteAppliedSegment();
    int elapsedSegments = currentAbsoluteSegment - appliedAbsoluteSegment;

    return elapsedSegments >= DurationSegments.Value;
}
}
