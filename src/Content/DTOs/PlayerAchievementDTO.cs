/// <summary>
/// DTO for Player Achievement instances - achievements earned by the player
/// Stored on Player save data, loaded into Player.EarnedAchievements
/// </summary>
public class PlayerAchievementDTO
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
    /// Values: "Morning", "Midday", "Afternoon", "Evening"
    /// </summary>
    public string EarnedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when achievement was earned
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
