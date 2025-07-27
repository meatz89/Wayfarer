using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Static factory for creating and initializing GameWorld instances.
/// Uses the new pipeline that gracefully handles missing references.
/// This is static to ensure clean initialization during startup without DI dependencies.
/// </summary>
public static class GameWorldInitializer
{
    /// <summary>
    /// Creates a new GameWorld instance using the default content directory.
    /// This method must be static to avoid circular dependencies during startup.
    /// </summary>
    public static GameWorld CreateGameWorld()
    {
        Console.WriteLine("[FACTORY] GameWorldInitializer.CreateGameWorld called");
        
        // Use default content directory path
        var contentDirectory = new ContentDirectory { Path = "Content" };
        
        Console.WriteLine("[FACTORY] LoadGameFromTemplates started - using new pipeline");
        
        // Use new pipeline that handles missing references gracefully
        var pipeline = new GameWorldInitializationPipeline(contentDirectory);
        var gameWorld = pipeline.Initialize();
        
        Console.WriteLine("[FACTORY] GameWorldInitializer.CreateGameWorld completed");
        return gameWorld;
    }
}