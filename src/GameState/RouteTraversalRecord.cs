
/// <summary>
/// Tracks LAST traversal timestamp per route for LeastRecent selection strategy
/// ONE record per route (update in place, not append-only)
/// LeastRecent strategy queries: "Which route was traveled LEAST recently?"
/// Enables procedural content to prefer routes player hasn't traveled recently
/// </summary>
public class RouteTraversalRecord
{
    /// <summary>
    /// Route entity this record tracks
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public RouteOption Route { get; set; }

    /// <summary>
    /// Day of last traversal on this route
    /// Updated each time player travels this route (replaces previous value)
    /// </summary>
    public int LastTraversalDay { get; set; }

    /// <summary>
    /// Time block of last traversal (Morning, Afternoon, Evening, Night)
    /// Updated each time player travels this route (replaces previous value)
    /// </summary>
    public TimeBlocks LastTraversalTimeBlock { get; set; }

    /// <summary>
    /// Segment of last traversal within time block (0-5)
    /// Updated each time player travels this route (replaces previous value)
    /// </summary>
    public int LastTraversalSegment { get; set; }
}
