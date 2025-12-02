/// <summary>
/// DTO for Achievement definitions - milestone markers players can earn
/// Achievements are earned and tracked as PlayerAchievement instances
/// </summary>
public class AchievementDTO
{
    /// <summary>
    /// Unique identifier for this achievement
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Category of this achievement
    /// Values: "Combat", "Social", "Investigation", "Economic", "Political"
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Display name of the achievement
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of how the achievement was earned
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Icon identifier for UI display
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Conditions required to grant this achievement
    /// Example condition types: "bondStrengthWithAnyNpc", "completedObligations", "moralityScale", "coinsEarned"
    /// </summary>
    public List<AchievementConditionEntry> GrantConditions { get; set; } = new List<AchievementConditionEntry>();
}
