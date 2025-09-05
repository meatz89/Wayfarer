using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Static factory for creating and initializing GameWorld instances.
/// Uses the new package-based loading system.
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

        Console.WriteLine("[FACTORY] LoadGameFromTemplates started - using new package-based system");

        // Create new GameWorld instance
        GameWorld gameWorld = new GameWorld();

        // Create PackageLoader and load content from the Core directory
        // AI-generated content will go in Content/Generated
        // Test packages are in Content/TestPackages
        PackageLoader packageLoader = new PackageLoader(gameWorld);
        packageLoader.LoadPackagesFromDirectory("Content/Core");

        Console.WriteLine("[FACTORY] GameWorldInitializer.CreateGameWorld completed");
        return gameWorld;
    }
}