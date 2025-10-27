using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
    /// Object references are resolved during parsing (HIGHLANDER Pattern A).
    /// </summary>
    public static GameWorld CreateGameWorld()
    {
        // Create new GameWorld instance
        GameWorld gameWorld = new GameWorld();

        // Load content from the Core directory
        // Parsers resolve object references during parsing
        // AI-generated content will go in Content/Generated
        // Test packages are in Content/TestPackages
        PackageLoader packageLoader = new PackageLoader(gameWorld);
        packageLoader.LoadPackagesFromDirectory("Content/Core");

        return gameWorld;
    }
}