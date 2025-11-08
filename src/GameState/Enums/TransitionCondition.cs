namespace Wayfarer.GameState.Enums;

/// <summary>
/// Condition types for when a Situation transition occurs
/// Defines what triggers advancement to next Situation in cascade
/// </summary>
public enum TransitionCondition
{
    /// <summary>
    /// Transition always occurs regardless of choice or outcome
    /// Automatic progression
    /// </summary>
    Always,

    /// <summary>
    /// Transition occurs only if specific choice selected
    /// Choice-specific branching
    /// </summary>
    OnChoice,

    /// <summary>
    /// Transition occurs if choice succeeded (reward applied)
    /// Success path
    /// </summary>
    OnSuccess,

    /// <summary>
    /// Transition occurs if choice failed
    /// Failure consequence path
    /// </summary>
    OnFailure
}
