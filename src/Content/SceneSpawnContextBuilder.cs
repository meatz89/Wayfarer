using Wayfarer.GameState.Enums;

namespace Wayfarer.Content;

/// <summary>
/// Static utility for building SceneSpawnContext from placement information
/// HIGHLANDER: ONE method for context building, used by all spawning code
/// Centralizes entity resolution logic (NPC/Location/Route lookups)
/// </summary>
public static class SceneSpawnContextBuilder
{
    /// <summary>
    /// Build SceneSpawnContext from placement information
    /// Resolves entity references from GameWorld based on PlacementRelation strategy
    /// Returns context with populated Player + placement-specific properties
    /// </summary>
    /// <param name="gameWorld">GameWorld for entity lookups</param>
    /// <param name="player">Player entity (always required)</param>
    /// <param name="placementRelation">Placement strategy (Specific* vs Same* vs Generic)</param>
    /// <param name="specificPlacementId">Entity ID for Specific* placement (NPC/Location/Route ID)</param>
    /// <param name="currentSituation">Optional parent Situation (null for starter/procedural scenes)</param>
    /// <returns>Context with Player and resolved entities, or null if entity not found</returns>
    public static SceneSpawnContext BuildContext(
        GameWorld gameWorld,
        Player player,
        PlacementRelation placementRelation,
        string specificPlacementId,
        Situation currentSituation = null)
    {
        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = player,
            CurrentSituation = currentSituation
        };

        switch (placementRelation)
        {
            case PlacementRelation.SpecificNPC:
                NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == specificPlacementId);
                if (npc == null)
                    return null;

                context.CurrentNPC = npc;
                context.CurrentLocation = npc.Location;
                break;

            case PlacementRelation.SpecificLocation:
                Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == specificPlacementId);
                if (location == null)
                    return null;

                context.CurrentLocation = location;
                break;

            case PlacementRelation.SpecificRoute:
                RouteOption route = gameWorld.Routes.FirstOrDefault(r => r.Id == specificPlacementId);
                if (route == null)
                    return null;

                context.CurrentRoute = route;

                if (route.OriginLocation != null)
                {
                    context.CurrentLocation = route.OriginLocation;
                }
                break;

            case PlacementRelation.SameLocation:
            case PlacementRelation.SameNPC:
            case PlacementRelation.SameRoute:
            case PlacementRelation.Generic:
                break;
        }

        return context;
    }
}
