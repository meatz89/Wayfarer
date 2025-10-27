
/// <summary>
/// Status of a situation in the Scene-Situation lifecycle
/// Tracks progression from dormant spawn through completion
/// </summary>
public enum SituationStatus
{
    /// <summary>
    /// Situation spawned but requirements not yet met
    /// Not visible in scene until requirements satisfied
    /// </summary>
    Dormant,

    /// <summary>
    /// Requirements met, situation visible and selectable
    /// </summary>
    Available,

    /// <summary>
    /// Situation selected, player currently engaged
    /// Used for challenge-type situations during gameplay
    /// </summary>
    Active,

    /// <summary>
    /// Situation resolved successfully
    /// Consequences applied, spawns executed
    /// </summary>
    Completed,

    /// <summary>
    /// Situation failed and no longer available
    /// Used for failed challenges that don't retry
    /// </summary>
    Failed
}
