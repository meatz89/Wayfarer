/// <summary>
/// Player Achievement instance - achievement earned by the player
/// Stored in Player.EarnedAchievements list with segment-based earned time
/// HIGHLANDER: Object reference to achievement definition
/// </summary>
public class PlayerAchievement
{
    /// <summary>
    /// Achievement definition - object reference to Achievement
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public Achievement Achievement { get; set; }

    /// <summary>
    /// Day when achievement was earned
    /// </summary>
    public int EarnedDay { get; set; }

    /// <summary>
    /// Time block when achievement was earned
    /// </summary>
    public TimeBlocks EarnedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when achievement was earned (1-4)
    /// </summary>
    public int EarnedSegment { get; set; }

    /// <summary>
    /// Related NPC if achievement was earned through NPC interaction
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public NPC RelatedNpc { get; set; }

    /// <summary>
    /// Related Location if achievement was earned at a specific location
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public Location RelatedLocation { get; set; }
}
