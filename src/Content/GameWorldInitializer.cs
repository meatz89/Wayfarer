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
        WireObjectGraph(gameWorld); return gameWorld;
    }

    /// <summary>
    /// Wire object graph - populate all object reference properties from ID properties
    /// This is PHASE 2 of initialization, executed AFTER all JSON parsing is complete
    /// </summary>
    private static void WireObjectGraph(GameWorld gameWorld)
    {// Wire Situations
        foreach (Situation situation in gameWorld.Situations)
        {
            if (!string.IsNullOrEmpty(situation.PlacementLocationId))
                situation.PlacementLocation = gameWorld.Locations.FirstOrDefault(l => l.Id == situation.PlacementLocationId);

            if (!string.IsNullOrEmpty(situation.PlacementNpcId))
                situation.PlacementNpc = gameWorld.NPCs.FirstOrDefault(n => n.ID == situation.PlacementNpcId);

            if (!string.IsNullOrEmpty(situation.ObligationId))
                situation.Obligation = gameWorld.Obligations.FirstOrDefault(i => i.Id == situation.ObligationId);
        }// Wire NPCs
        foreach (NPC npc in gameWorld.NPCs)
        {
            if (!string.IsNullOrEmpty(npc.LocationId))
                npc.Location = gameWorld.Locations.FirstOrDefault(l => l.Id == npc.LocationId);

            // ActiveSituations and Obstacles are accessed via ActiveSituationIds/ObstacleIds querying GameWorld.Situations/Obstacles
            // No need to populate duplicate object lists
        }

        // Wire ConversationTrees
        foreach (ConversationTree tree in gameWorld.ConversationTrees)
        {
            if (!string.IsNullOrEmpty(tree.NpcId))
                tree.Npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == tree.NpcId);
        }

        // Wire ObservationScenes
        foreach (ObservationScene scene in gameWorld.ObservationScenes)
        {
            if (!string.IsNullOrEmpty(scene.LocationId))
                scene.Location = gameWorld.Locations.FirstOrDefault(l => l.Id == scene.LocationId);
        }

        // Locations reference situations/obstacles via IDs only (ActiveSituationIds/ObstacleIds)
        // No object list population needed - GameWorld.Situations/Obstacles is single source of truth
    }
}