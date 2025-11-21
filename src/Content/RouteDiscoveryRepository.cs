/// <summary>
/// Provides access to route discovery data from the game world
/// HIGHLANDER: All methods accept typed objects, not string IDs
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
    /// HIGHLANDER: Accepts RouteOption object, not string routeId
    /// </summary>
    public RouteDiscovery? GetDiscoveryForRoute(RouteOption route)
    {
        return _gameWorld.RouteDiscoveries
            .FirstOrDefault(d => d.Route == route);
    }

    /// <summary>
    /// Get all routes that a specific NPC can teach
    /// HIGHLANDER: Accepts NPC object, not string npcId
    /// </summary>
    public List<RouteDiscovery> GetRoutesKnownByNPC(NPC npc)
    {
        return _gameWorld.RouteDiscoveries
            .Where(d => d.KnownByNPCs.Contains(npc))
            .ToList();
    }

    /// <summary>
    /// Get the discovery context for a specific NPC and route
    /// HIGHLANDER: Accepts RouteOption and NPC objects, not string IDs
    /// </summary>
    public RouteDiscoveryContext? GetDiscoveryContext(RouteOption route, NPC npc)
    {
        RouteDiscovery? discovery = GetDiscoveryForRoute(route);
        if (discovery != null && discovery.DiscoveryContexts.TryGetValue(npc, out RouteDiscoveryContext? context))
        {
            return context;
        }
        return null;
    }
}
