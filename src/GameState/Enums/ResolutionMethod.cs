namespace Wayfarer.GameState.Enums;

/// <summary>
/// How obstacle was overcome
/// Provides semantic context for AI narrative generation
/// </summary>
public enum ResolutionMethod
{
    /// <summary>
    /// Not yet overcome
    /// </summary>
    Unresolved,

    /// <summary>
    /// Forced, destroyed, attacked
    /// Physical confrontation, direct combat
    /// </summary>
    Violence,

    /// <summary>
    /// Negotiated, befriended, persuaded
    /// Social resolution, relationship building
    /// </summary>
    Diplomacy,

    /// <summary>
    /// Sneaked, avoided, bypassed undetected
    /// Stealth and subterfuge
    /// </summary>
    Stealth,

    /// <summary>
    /// Official channels, credentials, legal power
    /// Using formal authority and recognized status
    /// </summary>
    Authority,

    /// <summary>
    /// Outsmarted, found workaround, exploited weakness
    /// Mental problem-solving, clever tactics
    /// </summary>
    Cleverness,

    /// <summary>
    /// Methodical reduction over multiple attempts
    /// Incremental progress through Modify goals
    /// </summary>
    Preparation
}
