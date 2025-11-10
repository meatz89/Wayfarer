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
/// Get discoveries for a specific route
/// </summary>
public RouteDiscovery? GetDiscoveryForRoute(string routeId)
{
    return _gameWorld.RouteDiscoveries
        .FirstOrDefault(d => d.RouteId == routeId);
}

/// <summary>
/// Get all routes that a specific NPC can teach
/// </summary>
public List<RouteDiscovery> GetRoutesKnownByNPC(string npcId)
{
    return _gameWorld.RouteDiscoveries
        .Where(d => d.KnownByNPCs.Contains(npcId))
        .ToList();
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
