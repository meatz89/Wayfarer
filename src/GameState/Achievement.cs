using Wayfarer.GameState.Enums;

/// <summary>
/// Achievement definition - metadata about a milestone the player can earn
/// Loaded from 19_achievements.json, stored in GameWorld.Achievements
/// Not the instance - instances are PlayerAchievement in Player.EarnedAchievements
/// </summary>
public class Achievement
{
    /// <summary>
    /// Unique identifier for this achievement
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Category of this achievement
    /// </summary>
    public AchievementCategory Category { get; set; }

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
    /// Grant conditions - strongly-typed conditions required to earn this achievement
    /// Parsed from DTO's Dictionary into typed conditions
    /// Will be evaluated by ConsequenceFacade
    /// </summary>
    public AchievementGrantConditions GrantConditions { get; set; } = new AchievementGrantConditions();
}

/// <summary>
/// Strongly-typed grant conditions for achievements
/// Replaces generic Dictionary from DTO
/// </summary>
public class AchievementGrantConditions
{
    /// <summary>
    /// Minimum bond strength with any NPC
    /// </summary>
    public int? BondStrengthWithAnyNpc { get; set; }

    /// <summary>
    /// Number of completed obligations required
    /// </summary>
    public int? CompletedObligations { get; set; }

    /// <summary>
    /// Number of completed social situations required
    /// </summary>
    public int? CompletedSocialSituations { get; set; }

    /// <summary>
    /// Number of completed mental situations required
    /// </summary>
    public int? CompletedMentalSituations { get; set; }

    /// <summary>
    /// Number of completed physical situations required
    /// </summary>
    public int? CompletedPhysicalSituations { get; set; }

    /// <summary>
    /// Total coins earned (lifetime)
    /// </summary>
    public int? CoinsEarned { get; set; }

    /// <summary>
    /// Morality scale threshold (-10 to +10)
    /// Positive = altruistic, Negative = exploitative
    /// </summary>
    public int? MoralityScale { get; set; }

    /// <summary>
    /// Lawfulness scale threshold (-10 to +10)
    /// Positive = establishment, Negative = rebellious
    /// </summary>
    public int? LawfulnessScale { get; set; }

    /// <summary>
    /// Method scale threshold (-10 to +10)
    /// Positive = diplomatic, Negative = violent
    /// </summary>
    public int? MethodScale { get; set; }

    /// <summary>
    /// Caution scale threshold (-10 to +10)
    /// Positive = careful, Negative = reckless
    /// </summary>
    public int? CautionScale { get; set; }

    /// <summary>
    /// Transparency scale threshold (-10 to +10)
    /// Positive = open, Negative = secretive
    /// </summary>
    public int? TransparencyScale { get; set; }

    /// <summary>
    /// Fame scale threshold (-10 to +10)
    /// Positive = celebrated, Negative = notorious
    /// </summary>
    public int? FameScale { get; set; }
}
