using Wayfarer.Content;
using Wayfarer.GameState.Enums;

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
        Console.WriteLine("[VALIDATION-L1] GameWorldInitializer.CreateGameWorld() STARTED");

        // Create new GameWorld instance
        GameWorld gameWorld = new GameWorld();

        // Load content from the Core directory
        // Parsers resolve object references during parsing
        // AI-generated content will go in Content/Generated
        // Test packages are in Content/TestPackages
        Console.WriteLine("[VALIDATION-L1] Creating PackageLoader and loading from Content/Core");
        PackageLoader packageLoader = new PackageLoader(gameWorld);
        packageLoader.LoadPackagesFromDirectory("Content/Core");

        Console.WriteLine($"[VALIDATION-L1] Content loading COMPLETE - SceneTemplates: {gameWorld.SceneTemplates.Count}, Locations: {gameWorld.Locations.Count()}, NPCs: {gameWorld.NPCs.Count}");

        // Spawn initial starter Scenes to populate tutorial content
        SpawnInitialScenes(gameWorld);

        Console.WriteLine("[VALIDATION-L1] GameWorldInitializer.CreateGameWorld() COMPLETE");
        return gameWorld;
    }

    /// <summary>
    /// Spawn all SceneTemplates marked as starter content
    /// Creates Active Scenes (not provisional) to populate initial game world
    /// Called during game initialization after content loading
    /// </summary>
    private static void SpawnInitialScenes(GameWorld gameWorld)
    {
        Console.WriteLine("[VALIDATION-L2] SpawnInitialScenes() STARTED");

        SceneInstantiator instantiator = new SceneInstantiator(gameWorld);

        // Find all starter templates
        List<SceneTemplate> starterTemplates = gameWorld.SceneTemplates.Where(t => t.IsStarter).ToList();

        Console.WriteLine($"[VALIDATION-L2] Found {starterTemplates.Count} starter SceneTemplates to spawn");

        foreach (SceneTemplate template in starterTemplates)
        {
            Console.WriteLine($"[VALIDATION-L2] Spawning starter Scene from template: {template.Id}");

            // Build spawn reward for starter scene
            // Starter scenes use SIMPLE placement: first matching entity from filter
            SceneSpawnReward starterSpawn = new SceneSpawnReward
            {
                SceneTemplateId = template.Id,
                PlacementRelation = DeterminePlacementRelation(template),
                SpecificPlacementId = FindStarterPlacement(template, gameWorld),
                DelayDays = 0 // Starter content spawns immediately
            };

            // Build minimal context for starter spawning
            SceneSpawnContext context = BuildStarterContext(starterSpawn, gameWorld);

            if (context != null)
            {
                // Create Scene directly as Active (not provisional)
                Scene starterScene = instantiator.CreateProvisionalScene(template, starterSpawn, context);

                Console.WriteLine($"[VALIDATION-L2] Created Scene: {starterScene.Id} with {starterScene.SituationIds.Count} situations");

                // Immediately finalize (converts Provisional → Active, applies placeholder replacement)
                instantiator.FinalizeScene(starterScene.Id, context);

                Console.WriteLine($"[VALIDATION-L2] Finalized Scene: {starterScene.Id}");
            }
            else
            {
                Console.WriteLine($"[VALIDATION-L2] WARNING: Could not build context for starter template {template.Id}");
            }
        }

        Console.WriteLine($"[VALIDATION-L2] SpawnInitialScenes() COMPLETE - Total Scenes in GameWorld: {gameWorld.Scenes.Count}");
    }

    /// <summary>
    /// Determine PlacementRelation from template's PlacementFilter
    /// Simple implementation: Use SpecificPlacement based on filter's placementType
    /// </summary>
    private static PlacementRelation DeterminePlacementRelation(SceneTemplate template)
    {
        if (template.PlacementFilter == null)
            return PlacementRelation.SpecificLocation; // Default to location

        string placementType = template.PlacementFilter.PlacementType.ToString().ToLowerInvariant();

        return placementType switch
        {
            "location" => PlacementRelation.SpecificLocation,
            "npc" => PlacementRelation.SpecificNPC,
            "route" => PlacementRelation.SpecificRoute,
            _ => PlacementRelation.SpecificLocation
        };
    }

    /// <summary>
    /// Find concrete placement entity for starter Scene
    /// SIMPLE VERSION: Returns first matching entity from PlacementFilter
    /// TODO: Implement full PlacementFilter evaluation (personality, tags, bond thresholds)
    /// </summary>
    private static string FindStarterPlacement(SceneTemplate template, GameWorld gameWorld)
    {
        if (template.PlacementFilter == null)
        {
            // No filter: use first available entity
            return gameWorld.Locations.FirstOrDefault()?.Id;
        }

        string placementType = template.PlacementFilter.PlacementType.ToString().ToLowerInvariant();

        return placementType switch
        {
            "location" => gameWorld.Locations.FirstOrDefault()?.Id,
            "npc" => gameWorld.NPCs.FirstOrDefault()?.ID,
            "route" => gameWorld.Routes.FirstOrDefault()?.Id,
            _ => gameWorld.Locations.FirstOrDefault()?.Id
        };
    }

    /// <summary>
    /// Build SceneSpawnContext for starter Scene spawning
    /// Resolves placement entity from ID
    /// </summary>
    private static SceneSpawnContext BuildStarterContext(SceneSpawnReward spawn, GameWorld gameWorld)
    {
        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = gameWorld.GetPlayer(),
            CurrentSituation = null // Starter scenes spawn independent of Situations
        };

        switch (spawn.PlacementRelation)
        {
            case PlacementRelation.SpecificLocation:
                Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == spawn.SpecificPlacementId);
                if (location == null) return null;

                Venue venue = gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId);
                if (venue == null) return null;

                context.CurrentLocation = venue;
                break;

            case PlacementRelation.SpecificNPC:
                NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == spawn.SpecificPlacementId);
                if (npc == null) return null;
                context.CurrentNPC = npc;
                break;

            case PlacementRelation.SpecificRoute:
                RouteOption route = gameWorld.Routes.FirstOrDefault(r => r.Id == spawn.SpecificPlacementId);
                if (route == null) return null;
                context.CurrentRoute = route;
                break;

            default:
                return null;
        }

        return context;
    }
}