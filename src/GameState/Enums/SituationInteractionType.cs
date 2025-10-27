/// <summary>
/// Type of interaction when player selects a situation
/// Determines how the situation is resolved (distinct from UI InteractionType)
/// </summary>
public enum SituationInteractionType
{
    /// <summary>
    /// Resolves immediately without challenge - instant consequences
    /// Example: Accept job offer, make quick decision, trigger event
    /// </summary>
    Instant,

    /// <summary>
    /// Launches a Mental challenge (investigation/puzzle)
    /// Player navigates to challenge screen
    /// </summary>
    Mental,

    /// <summary>
    /// Launches a Physical challenge (obstacle/endurance)
    /// Player navigates to challenge screen
    /// </summary>
    Physical,

    /// <summary>
    /// Launches a Social challenge (conversation/persuasion)
    /// Player navigates to challenge screen
    /// </summary>
    Social,

    /// <summary>
    /// Navigation action - moves player to different location
    /// May trigger scene at destination
    /// </summary>
    Navigation
}
