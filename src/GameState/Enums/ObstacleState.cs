namespace Wayfarer.GameState.Enums;

/// <summary>
/// Current state of obstacle
/// </summary>
public enum ObstacleState
{
    /// <summary>
    /// Currently blocking, player must interact to progress
    /// </summary>
    Active,

    /// <summary>
    /// Permanently overcome, removed from play
    /// Achieved through Resolution consequence
    /// </summary>
    Resolved,

    /// <summary>
    /// Fundamentally changed, still exists but neutral
    /// Properties set to zero, new description
    /// Achieved through Transform consequence or complete Modify sequence
    /// </summary>
    Transformed
}
