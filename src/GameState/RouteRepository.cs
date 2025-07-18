using System.Linq;
public class RouteRepository
{
    private readonly GameWorld _gameWorld;

    public RouteRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    // Check if a route is blocked
    public bool IsRouteBlocked(string routeId)
    {
        return _gameWorld.WorldState.IsRouteBlocked(routeId);
    }

    // Get current weather (weather affects route availability)
    public WeatherCondition GetCurrentWeather()
    {
        return _gameWorld.WorldState.CurrentWeather;
    }

    // Get routes from a specific location
    public List<RouteOption> GetRoutesFromLocation(string locationId)
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
    public List<RouteOption> GetAllRoutes()
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
        var allRoutes = GetAllRoutes();
        return allRoutes.FirstOrDefault(r => r.Id == routeId);
    }
}