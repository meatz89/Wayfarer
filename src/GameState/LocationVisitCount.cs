using Wayfarer.GameState.Enums;

/// <summary>
/// Tracks LAST visit timestamp per location for LeastRecent selection strategy
/// ONE record per location (update in place, not append-only)
/// LeastRecent strategy queries: "Which location was visited LEAST recently?"
/// Enables procedural content to prefer locations player hasn't visited recently
/// Renamed from LocationVisitCount (which only tracked count, not timestamp)
/// </summary>
public class LocationVisitRecord
{
    /// <summary>
    /// Location entity ID this record tracks
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// Day of last visit to this location
    /// Updated each time player enters this location (replaces previous value)
    /// </summary>
    public int LastVisitDay { get; set; }

    /// <summary>
    /// Time block of last visit (Morning, Afternoon, Evening, Night)
    /// Updated each time player enters this location (replaces previous value)
    /// </summary>
    public TimeBlocks LastVisitTimeBlock { get; set; }

    /// <summary>
    /// Segment of last visit within time block (0-5)
    /// Updated each time player enters this location (replaces previous value)
    /// </summary>
    public int LastVisitSegment { get; set; }
}
