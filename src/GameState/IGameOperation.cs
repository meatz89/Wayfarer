namespace Wayfarer.GameState;

/// <summary>
/// Represents an atomic game operation that can be executed and rolled back
/// </summary>
public interface IGameOperation
{
    /// <summary>
    /// Validates whether this operation can be executed in the current game state
    /// </summary>
    bool CanExecute(GameWorld gameWorld);
    
    /// <summary>
    /// Executes the operation, making changes to the game state
    /// </summary>
    void Execute(GameWorld gameWorld);
    
    /// <summary>
    /// Rolls back the operation, reverting any changes made during Execute
    /// </summary>
    void Rollback(GameWorld gameWorld);
    
    /// <summary>
    /// Gets a description of what this operation does
    /// </summary>
    string Description { get; }
}