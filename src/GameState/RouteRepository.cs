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

    // Get current weather (weather affects route availability)
    public WeatherCondition GetCurrentWeather()
    {
        return _gameWorld.WorldState.CurrentWeather;
    }

    // Get routes from a specific location (by checking all spots in that location)
    public IEnumerable<RouteOption> GetRoutesFromLocation(string locationId)
    {
        List<RouteOption> allRoutes = new List<RouteOption>();

        // Find the location and get its connections (which are routes originating from that location)
        if (_gameWorld.WorldState.locations != null)
        {
            Location? location = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
            if (location?.Connections != null)
            {
                foreach (LocationConnection connection in location.Connections)
                {
                    if (connection.RouteOptions != null)
                    {
                        allRoutes.AddRange(connection.RouteOptions);
                    }
                }
            }
        }

        return allRoutes;
    }

    // Get all routes in the world
    public List<RouteOption> GetAll()
    {
        List<RouteOption> allRoutes = new List<RouteOption>();

        if (_gameWorld.WorldState.locations != null)
        {
            foreach (Location location in _gameWorld.WorldState.locations)
            {
                if (location.Connections != null)
                {
                    foreach (LocationConnection connection in location.Connections)
                    {
                        if (connection.RouteOptions != null)
                        {
                            allRoutes.AddRange(connection.RouteOptions);
                        }
                    }
                }
            }
        }

        return allRoutes;
    }

    // Get a specific route by ID
    public RouteOption GetRouteById(string routeId)
    {
        List<RouteOption> allRoutes = GetAll();
        return allRoutes.FirstOrDefault(r => r.Id == routeId);
    }

    // Get available routes from the player's current spot
    public IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player)
    {
        // Get routes from the player's current spot
        LocationSpot currentSpot = player.CurrentLocationSpot;
        if (currentSpot == null) return new List<RouteOption>();

        // Get all routes that start from the current spot
        IEnumerable<RouteOption> allRoutes = GetAll().Where(r => r.OriginLocationSpot == currentSpot.SpotID);
        List<RouteOption> availableRoutes = new List<RouteOption>();

        foreach (RouteOption route in allRoutes)
        {
            // Check if route is discovered
            if (!route.IsDiscovered)
                continue;

            // Check if route has special access requirements (unless unlocked by permit)
            if (route.AccessRequirement != null && !route.HasPermitUnlock)
                continue;

            // Check if route is blocked
            if (IsRouteBlocked(route.Id))
                continue;

            // Check access requirements if any
            if (route.AccessRequirement != null)
            {
                // For now, just check if permit has been received
                // Transport permits will be handled in task 4
                if (!route.AccessRequirement.HasReceivedPermit)
                    continue;
            }

            availableRoutes.Add(route);
        }

        return availableRoutes;
    }

    // Check if player has required equipment for a route
    public bool PlayerHasRequiredEquipment(RouteOption route, Player player)
    {
        // This checks terrain category requirements
        // Already implemented in RouteOption.CheckRouteAccess
        RouteAccessResult result = route.CheckRouteAccess(
            _itemRepository,
            player,
            GetCurrentWeather()
        );

        return result.IsAllowed;
    }
}