/// <summary>
/// Factory interface for creating GameWorld instances.
/// This allows proper dependency injection without circular dependencies.
/// </summary>
public interface IGameWorldFactory
{
    /// <summary>
    /// Creates and initializes a new GameWorld instance loaded from JSON content.
    /// </summary>
    GameWorld CreateGameWorld();
}