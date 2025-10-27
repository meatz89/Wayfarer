/// <summary>
/// DTO for Active State instances - temporary conditions currently affecting the player
/// Stored on Player save data, loaded into Player.ActiveStates
/// </summary>
public class ActiveStateDTO
{
    /// <summary>
    /// State type (must match StateType enum)
    /// Values: "Wounded", "Exhausted", "Sick", etc.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Category of this state
    /// Values: "Physical", "Mental", "Social"
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Day when state was applied
    /// </summary>
    public int AppliedDay { get; set; }

    /// <summary>
    /// Time block when state was applied
    /// Values: "Morning", "Midday", "Afternoon", "Evening"
    /// </summary>
    public string AppliedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when state was applied
    /// </summary>
    public int AppliedSegment { get; set; }

    /// <summary>
    /// Duration in segments before state auto-clears
    /// null = does not auto-clear, must be cleared manually or by condition
    /// </summary>
    public int? DurationSegments { get; set; }
}
