using System.Linq;
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
        return _gameWorld.WorldState.IsRouteBlocked(routeId, _gameWorld.CurrentDay);
    }


    // Get routes from a specific Venue (by checking all Locations in that location)
    public IEnumerable<RouteOption> GetRoutesFromLocation(string venueId)
    {
        List<RouteOption> allRoutes = new List<RouteOption>();

        // ONLY use WorldState.Routes as the single source of truth
        if (_gameWorld.WorldState.Routes != null)
        {
            // Find all routes that originate from any location in this location
            foreach (RouteOption route in _gameWorld.WorldState.Routes)
            {
                // Look up the origin location to get its location
                if (!string.IsNullOrEmpty(route.OriginLocationSpot))
                {
                    Location originSpot = _gameWorld.GetLocation(route.OriginLocationSpot);
                    if (originSpot != null && originSpot.VenueId == venueId)
                    {
                        allRoutes.Add(route);
                    }
                }
            }
        }

        return allRoutes;
    }

    // Get all routes in the world
    public List<RouteOption> GetAll()
    {
        // ONLY use WorldState.Routes as the single source of truth
        return _gameWorld.WorldState.Routes ?? new List<RouteOption>();
    }

    // Get a specific route by ID
    public RouteOption GetRouteById(string routeId)
    {
        List<RouteOption> allRoutes = GetAll();
        return allRoutes.FirstOrDefault(r => r.Id == routeId);
    }

    // Get available routes from the player's current location
    public IEnumerable<RouteOption> GetAvailableRoutes(string fromVenueId, Player player)
    {
        // Get routes from the player's current location
        Location currentSpot = player.CurrentLocation;
        if (currentSpot == null) return new List<RouteOption>();

        // Get all routes that start from the current location
        IEnumerable<RouteOption> allRoutes = GetAll().Where(r => r.OriginLocationSpot == currentSpot.Id);
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