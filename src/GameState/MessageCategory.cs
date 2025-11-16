namespace Wayfarer.GameState;

/// <summary>
/// Semantic category of a system message, indicating WHAT type of event occurred.
/// Backend provides domain meaning through this enum.
/// Frontend maps categories to visual presentation (icons, styling).
/// </summary>
public enum MessageCategory
{
    /// <summary>
    /// Resource changes: coins earned/spent, health gained/lost, hunger increased/decreased, items acquired/lost
    /// </summary>
    ResourceChange,

    /// <summary>
    /// Time progression: time blocks advancing, day changes, time-related events
    /// </summary>
    TimeProgression,

    /// <summary>
    /// Discovery events: investigations, observations revealed, secrets uncovered
    /// </summary>
    Discovery,

    /// <summary>
    /// Achievements: unlocks, milestones, new capabilities gained
    /// </summary>
    Achievement,

    /// <summary>
    /// Danger events: warnings, death, hostility, critical failures
    /// </summary>
    Danger,

    /// <summary>
    /// Narrative events: letter obligations, story beats, quest updates
    /// </summary>
    Narrative,

    /// <summary>
    /// Social events: relationship changes, betrayal, diplomacy, social interactions
    /// </summary>
    Social
}
