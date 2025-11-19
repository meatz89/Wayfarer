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
    // HIGHLANDER: Accepts RouteOption object, delegates to GameWorld
    public bool IsRouteBlocked(RouteOption route)
    {
        return _gameWorld.IsRouteBlocked(route, _gameWorld.CurrentDay);
    }

    // Get routes from a specific Location
    // HIGHLANDER: Accept Location object, compare objects
    public IEnumerable<RouteOption> GetRoutesFromLocation(Location location)
    {
        // ONLY use GameWorld.Routes as the single source of truth
        if (_gameWorld.Routes == null)
            return new List<RouteOption>();

        // Query by OriginLocation object reference (object equality)
        return _gameWorld.Routes.Where(r => r.OriginLocation == location);
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
    // HIGHLANDER: Accept Location object
    public IEnumerable<RouteOption> GetAvailableRoutes(Location fromLocation, Player player)
    {
        if (fromLocation == null) return new List<RouteOption>();

        // Get all routes that start from this location
        IEnumerable<RouteOption> allRoutes = GetRoutesFromLocation(fromLocation);
        List<RouteOption> availableRoutes = new List<RouteOption>();

        foreach (RouteOption route in allRoutes)
        {
            // Core Loop: All routes physically exist and are visible
            // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access

            // Check if route is blocked (HIGHLANDER: pass object, not Name)
            if (IsRouteBlocked(route))
                continue;

            availableRoutes.Add(route);
        }

        return availableRoutes;
    }

}