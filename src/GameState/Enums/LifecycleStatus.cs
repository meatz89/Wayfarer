namespace Wayfarer.GameState.Enums;

/// <summary>
/// Tracks situation progression from spawn through resolution
/// Part of lifecycle management, orthogonal to InstantiationState
/// Controls UI visibility, selectability, and completion state
/// </summary>
public enum LifecycleStatus
{
    /// <summary>
    /// Situation spawned but CompoundRequirements not yet met
    /// Not visible in scene UI until requirements satisfied
    /// Invisible to player, blocked by prerequisites
    /// </summary>
    Locked,

    /// <summary>
    /// CompoundRequirements met, situation visible and selectable in UI
    /// Player can see and select this situation
    /// Displayed in scene interface as available action
    /// </summary>
    Selectable,

    /// <summary>
    /// Player selected situation, currently engaged in execution
    /// Used for challenge-type situations during tactical gameplay
    /// Challenge subsystem executing (Social/Mental/Physical)
    /// </summary>
    InProgress,

    /// <summary>
    /// Situation resolved successfully
    /// Consequences applied, spawns executed
    /// ProjectedBondChanges/ScaleShifts/States applied to player
    /// </summary>
    Completed,

    /// <summary>
    /// Situation attempt failed
    /// Used for failed challenges
    /// Player can retry if Repeatable=true
    /// </summary>
    Failed
}
