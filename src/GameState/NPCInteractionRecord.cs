
/// <summary>
/// Tracks LAST interaction timestamp per NPC for LeastRecent selection strategy
/// ONE record per NPC (update in place, not append-only)
/// LeastRecent strategy queries: "Which NPC was interacted with LEAST recently?"
/// Enables procedural content to prefer NPCs player hasn't interacted with recently
/// HIGHLANDER: Object reference, not string ID
/// </summary>
public class NPCInteractionRecord
{
    /// <summary>
    /// NPC entity this record tracks
    /// </summary>
    public NPC Npc { get; set; }

    /// <summary>
    /// Day of last interaction with this NPC
    /// Updated each time player interacts with this NPC (replaces previous value)
    /// </summary>
    public int LastInteractionDay { get; set; }

    /// <summary>
    /// Time block of last interaction (Morning, Afternoon, Evening, Night)
    /// Updated each time player interacts with this NPC (replaces previous value)
    /// </summary>
    public TimeBlocks LastInteractionTimeBlock { get; set; }

    /// <summary>
    /// Segment of last interaction within time block (0-5)
    /// Updated each time player interacts with this NPC (replaces previous value)
    /// </summary>
    public int LastInteractionSegment { get; set; }
}
