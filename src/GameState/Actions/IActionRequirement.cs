namespace Wayfarer.GameState.Actions;

/// <summary>
/// Interface for action requirements that can be validated against player state
/// </summary>
public interface IActionRequirement
{
    /// <summary>
    /// Check if this requirement is satisfied by the player
    /// </summary>
    bool IsSatisfied(Player player, GameWorld world);
    
    /// <summary>
    /// Get a user-friendly description of what this requirement needs
    /// </summary>
    string GetDescription();
    
    /// <summary>
    /// Get the reason why this requirement is not satisfied
    /// </summary>
    string GetFailureReason(Player player, GameWorld world);
    
    /// <summary>
    /// Whether the player can take actions to remedy this requirement
    /// </summary>
    bool CanBeRemedied { get; }
    
    /// <summary>
    /// Get a hint about how to satisfy this requirement
    /// </summary>
    string GetRemediationHint();
    
    /// <summary>
    /// Get progress towards satisfying this requirement (0.0 to 1.0)
    /// </summary>
    double GetProgress(Player player, GameWorld world);
}