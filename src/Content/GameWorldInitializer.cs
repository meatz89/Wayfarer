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
    /// </summary>
    public static GameWorld CreateGameWorld()
    {// Create new GameWorld instance
        GameWorld gameWorld = new GameWorld();

        // Create PackageLoader and load content from the Core directory
        // PHASE 1: Parse all JSON → Domain entities (ID properties populated)
        // AI-generated content will go in Content/Generated
        // Test packages are in Content/TestPackages
        PackageLoader packageLoader = new PackageLoader(gameWorld);
        packageLoader.LoadPackagesFromDirectory("Content/Core");

        // PHASE 2: Wire object graph (populate object reference properties)
        WireObjectGraph(gameWorld);return gameWorld;
    }

    /// <summary>
    /// Wire object graph - populate all object reference properties from ID properties
    /// This is PHASE 2 of initialization, executed AFTER all JSON parsing is complete
    /// </summary>
    private static void WireObjectGraph(GameWorld gameWorld)
    {// Wire Goals
        foreach (Goal goal in gameWorld.Goals)
        {
            if (!string.IsNullOrEmpty(goal.PlacementLocationId))
                goal.PlacementLocation = gameWorld.Locations.FirstOrDefault(l => l.Id == goal.PlacementLocationId);

            if (!string.IsNullOrEmpty(goal.PlacementNpcId))
                goal.PlacementNpc = gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);

            if (!string.IsNullOrEmpty(goal.InvestigationId))
                goal.Investigation = gameWorld.Investigations.FirstOrDefault(i => i.Id == goal.InvestigationId);
        }// Wire NPCs
        foreach (NPC npc in gameWorld.NPCs)
        {
            if (!string.IsNullOrEmpty(npc.LocationId))
                npc.Location = gameWorld.Locations.FirstOrDefault(l => l.Id == npc.LocationId);

            npc.ActiveGoals = gameWorld.Goals.Where(g => npc.ActiveGoalIds != null && npc.ActiveGoalIds.Contains(g.Id)).ToList();
            npc.Obstacles = gameWorld.Obstacles.Where(o => npc.ObstacleIds != null && npc.ObstacleIds.Contains(o.Id)).ToList();
        }// Wire Locations
        foreach (Location location in gameWorld.Locations)
        {
            location.ActiveGoals = gameWorld.Goals.Where(g => location.ActiveGoalIds != null && location.ActiveGoalIds.Contains(g.Id)).ToList();
            location.Obstacles = gameWorld.Obstacles.Where(o => location.ObstacleIds != null && location.ObstacleIds.Contains(o.Id)).ToList();
        }}
}