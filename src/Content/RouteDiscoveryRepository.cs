using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides access to route discovery data from the game world
/// </summary>
public class RouteDiscoveryRepository
{
    private readonly GameWorld _gameWorld;

    public RouteDiscoveryRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Get all route discoveries
    /// </summary>
    public List<RouteDiscovery> GetAllRouteDiscoveries()
    {
        return _gameWorld.WorldState.RouteDiscoveries;
    }

    /// <summary>
    /// Get discoveries for a specific route
    /// </summary>
    public RouteDiscovery? GetDiscoveryForRoute(string routeId)
    {
        return _gameWorld.WorldState.RouteDiscoveries
            .FirstOrDefault(d => d.RouteId == routeId);
    }

    /// <summary>
    /// Get all routes that a specific NPC can teach
    /// </summary>
    public List<RouteDiscovery> GetRoutesKnownByNPC(string npcId)
    {
        return _gameWorld.WorldState.RouteDiscoveries
            .Where(d => d.KnownByNPCs.Contains(npcId))
            .ToList();
    }

    /// <summary>
    /// Check if a specific NPC knows about a route
    /// </summary>
    public bool DoesNPCKnowRoute(string npcId, string routeId)
    {
        RouteDiscovery? discovery = GetDiscoveryForRoute(routeId);
        return discovery != null && discovery.KnownByNPCs.Contains(npcId);
    }

    /// <summary>
    /// Get the discovery context for a specific NPC and route
    /// </summary>
    public RouteDiscoveryContext? GetDiscoveryContext(string routeId, string npcId)
    {
        RouteDiscovery? discovery = GetDiscoveryForRoute(routeId);
        if (discovery != null && discovery.DiscoveryContexts.TryGetValue(npcId, out RouteDiscoveryContext? context))
        {
            return context;
        }
        return null;
    }
}
