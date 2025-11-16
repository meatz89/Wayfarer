public class RouteRepository : IRouteRepository
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;

    public RouteRepository(GameWorld gameWorld, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
    }

    // Check if a route is blocked
    public bool IsRouteBlocked(string routeId)
    {
        return _gameWorld.IsRouteBlocked(routeId, _gameWorld.CurrentDay);
    }

    // Get routes from a specific Location
    public IEnumerable<RouteOption> GetRoutesFromLocation(string locationId)
    {
        // ONLY use GameWorld.Routes as the single source of truth
        if (_gameWorld.Routes == null)
            return new List<RouteOption>();

        // Direct query by OriginLocation (no iteration needed)
        return _gameWorld.Routes.Where(r => r.OriginLocationId == locationId);
    }

    // Get all routes in the world
    public List<RouteOption> GetAll()
    {
        // ONLY use GameWorld.Routes as the single source of truth
        if (_gameWorld.Routes == null)
            throw new System.InvalidOperationException("GameWorld.Routes is null - package content not loaded");
        return _gameWorld.Routes;
    }


    // Get available routes from the player's current location
    public IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player)
    {
        // Get routes from the specified location
        Location location = _gameWorld.GetLocation(fromLocationId);
        if (location == null) return new List<RouteOption>();

        // Get all routes that start from this location
        IEnumerable<RouteOption> allRoutes = GetRoutesFromLocation(location.Id);
        List<RouteOption> availableRoutes = new List<RouteOption>();

        foreach (RouteOption route in allRoutes)
        {
            // Core Loop: All routes physically exist and are visible
            // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access

            // Check if route is blocked
            if (IsRouteBlocked(route.Id))
                continue;

            availableRoutes.Add(route);
        }

        return availableRoutes;
    }

}