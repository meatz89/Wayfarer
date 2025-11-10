/// <summary>
/// Player Achievement instance - achievement earned by the player
/// Stored in Player.EarnedAchievements list with segment-based earned time
/// References achievement definition by ID
/// </summary>
public class PlayerAchievement
{
/// <summary>
/// Achievement ID - references achievement definition in 19_achievements.json
/// </summary>
public string AchievementId { get; set; }

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
/// Related NPC ID if achievement was earned through NPC interaction
/// null if not NPC-related
/// </summary>
public string RelatedNpcId { get; set; }

/// <summary>
/// Related Location ID if achievement was earned at a specific location
/// null if not location-specific
/// </summary>
public string RelatedLocationId { get; set; }
}
