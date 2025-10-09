/// <summary>
/// Single bonus source for transparent effect calculation.
/// Perfect information principle: Players must see ALL bonuses that affect their actions.
/// </summary>
public class EffectBonus
{
    /// <summary>
    /// Human-readable bonus source description displayed to player.
    /// Examples: "Combat Specialist", "Authority Level 5", "Steel Sword", "Combat Mastery"
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Bonus amount (positive or negative).
    /// Examples: +2 Breakthrough, -1 Danger, +1 Position
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Bonus type for categorization and filtering.
    /// </summary>
    public BonusType Type { get; set; }
}

/// <summary>
/// Bonus type categories for effect bonuses.
/// Each bonus type represents a different source of power/advantage.
/// </summary>
public enum BonusType
{
    /// <summary>
    /// Card discipline matches challenge type (specialist bonus)
    /// Physical: Combat card in Combat challenge
    /// Mental: Research card in Research investigation
    /// </summary>
    Discipline,

    /// <summary>
    /// Player stat level bonus (progression system)
    /// Higher stat levels grant bonuses to cards bound to that stat
    /// </summary>
    StatLevel,

    /// <summary>
    /// Mastery token bonus (Physical - repeated challenge success)
    /// Reduces Danger at familiar challenge types
    /// </summary>
    Mastery,

    /// <summary>
    /// Familiarity bonus (Mental - repeated Venue investigation)
    /// Reduces Exposure at familiar locations
    /// </summary>
    Familiarity,

    /// <summary>
    /// Equipment bonus (items providing tactical advantages)
    /// Weapons, tools, gear enhancing specific actions
    /// </summary>
    Equipment,

    /// <summary>
    /// Investigation profile bonus (Mental - location-specific)
    /// Cards matching location's investigation style get bonuses
    /// </summary>
    Profile,

    /// <summary>
    /// Personality bonus (Social - NPC-specific)
    /// Cards matching NPC personality preferences get bonuses
    /// </summary>
    Personality,

    /// <summary>
    /// Connection bonus (Social - relationship-based)
    /// High connection levels provide bonuses to certain interactions
    /// </summary>
    Connection,

    /// <summary>
    /// Exertion state bonus/penalty (all systems)
    /// Low stamina/health affects costs and risks
    /// </summary>
    Exertion,

    /// <summary>
    /// Other/miscellaneous bonuses not fitting above categories
    /// </summary>
    Other
}
