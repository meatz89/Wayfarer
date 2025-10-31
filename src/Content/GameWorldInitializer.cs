using Wayfarer.Content;
using Wayfarer.GameState.Enums;
using Wayfarer.Services;

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

        // Generate procedural routes from hex grid pathfinding
        // Must happen AFTER package loading (locations + hex grid must exist)
        // Must happen BEFORE scene spawning (route-based scenes need routes)
        GenerateProceduralRoutes(gameWorld);

        // Spawn initial starter Scenes to populate tutorial content
        SpawnInitialScenes(gameWorld);

        return gameWorld;
    }

    /// <summary>
    /// Generate all routes between locations using hex-based pathfinding
    /// Called after package loading (so locations + hex grid exist)
    /// Called before scene spawning (route-based scenes need routes to exist)
    /// </summary>
    private static void GenerateProceduralRoutes(GameWorld gameWorld)
    {
        HexRouteGenerator routeGenerator = new HexRouteGenerator(gameWorld);

        // Generate all routes between locations with different venue membership
        List<RouteOption> generatedRoutes = routeGenerator.GenerateAllRoutes();

        // Add routes to GameWorld (single source of truth)
        gameWorld.Routes.AddRange(generatedRoutes);

        // Log route generation summary
        int locationCount = gameWorld.Locations.Count(loc => loc.HexPosition.HasValue);
        Console.WriteLine($"[Route Generation] Generated {generatedRoutes.Count} routes connecting {locationCount} locations with hex positions");
    }

    /// <summary>
    /// Spawn all SceneTemplates marked as starter content
    /// Creates Active Scenes (not provisional) to populate initial game world
    /// Called during game initialization after content loading
    /// </summary>
    private static void SpawnInitialScenes(GameWorld gameWorld)
    {
        SceneInstantiator instantiator = new SceneInstantiator(gameWorld);

        // Find all starter templates
        List<SceneTemplate> starterTemplates = gameWorld.SceneTemplates.Where(t => t.IsStarter).ToList();

        foreach (SceneTemplate template in starterTemplates)
        {
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

                // Immediately finalize (converts Provisional → Active, applies placeholder replacement)
                instantiator.FinalizeScene(starterScene.Id, context);
            }
        }
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
    /// Evaluates PlacementFilter criteria: personality, bond thresholds, tags
    /// Returns first matching entity that satisfies all filter conditions
    /// </summary>
    private static string FindStarterPlacement(SceneTemplate template, GameWorld gameWorld)
    {
        if (template.PlacementFilter == null)
        {
            // No filter: use first available entity
            return gameWorld.Locations.FirstOrDefault()?.Id;
        }

        PlacementFilter filter = template.PlacementFilter;
        string placementType = filter.PlacementType.ToString().ToLowerInvariant();
        Player player = gameWorld.GetPlayer();

        switch (placementType)
        {
            case "location":
                return gameWorld.Locations.FirstOrDefault()?.Id;

            case "npc":
                // Filter NPCs by PlacementFilter criteria
                NPC matchingNPC = gameWorld.NPCs.FirstOrDefault(npc =>
                {
                    // Check personality type (if specified)
                    if (filter.PersonalityTypes != null && filter.PersonalityTypes.Count > 0)
                    {
                        if (!filter.PersonalityTypes.Contains(npc.PersonalityType))
                            return false;
                    }

                    // Check bond thresholds (BondStrength stored directly on NPC)
                    int currentBond = npc.BondStrength;
                    if (filter.MinBond.HasValue && currentBond < filter.MinBond.Value)
                        return false;
                    if (filter.MaxBond.HasValue && currentBond > filter.MaxBond.Value)
                        return false;

                    // NPC tags check removed - NPCs don't have Tags property in current architecture
                    // TODO: Add Tags property to NPC if needed for future PlacementFilter scenarios

                    return true;
                });

                return matchingNPC?.ID;

            case "route":
                return gameWorld.Routes.FirstOrDefault()?.Id;

            default:
                return gameWorld.Locations.FirstOrDefault()?.Id;
        }
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