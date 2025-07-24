using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Factory for creating and initializing GameWorld instances.
/// Uses the new pipeline that gracefully handles missing references.
/// </summary>
public class GameWorldInitializer : IGameWorldFactory
{
    private readonly IContentDirectory _contentDirectory;
    private readonly ILogger<GameWorldInitializer> _logger;

    public GameWorldInitializer(
        IContentDirectory contentDirectory,
        ILogger<GameWorldInitializer> logger = null)
    {
        _contentDirectory = contentDirectory;
        _logger = logger;
    }

    public GameWorld LoadGame()
    {
        return LoadGameFromTemplates();
    }

    /// <summary>
    /// IGameWorldFactory implementation
    /// </summary>
    public GameWorld CreateGameWorld()
    {
        Console.WriteLine("[FACTORY] GameWorldInitializer.CreateGameWorld called");
        _logger?.LogInformation("CreateGameWorld method started");

        GameWorld gameWorld = LoadGame();

        _logger?.LogInformation("CreateGameWorld method completed, GameWorld instance created");
        Console.WriteLine("[FACTORY] GameWorldInitializer.CreateGameWorld completed");
        return gameWorld;
    }

    private GameWorld LoadGameFromTemplates()
    {
        Console.WriteLine("[FACTORY] LoadGameFromTemplates started - using new pipeline");
        
        // Use new pipeline that handles missing references gracefully
        var pipeline = new GameWorldInitializationPipeline(_contentDirectory);
        return pipeline.Initialize();
    }
}