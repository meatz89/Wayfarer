/// <summary>
/// Manages route operations and availability.
/// </summary>
public class RouteManager
{
    private readonly RouteRepository _routeRepository;
    private readonly GameWorld _gameWorld;

    public RouteManager(RouteRepository routeRepository, GameWorld gameWorld)
    {
        _routeRepository = routeRepository;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Get all routes from a specific location.
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public List<RouteOption> GetRoutesFromLocation(Location location)
    {
        return _routeRepository.GetRoutesFromLocation(location).ToList();
    }

    /// <summary>
    /// Get a specific route between two locations.
    /// HIGHLANDER: Accept Location objects, use object equality
    /// </summary>
    public RouteOption GetRouteBetweenLocations(Location fromLocation, Location toLocation)
    {
        if (fromLocation == null || toLocation == null)
            return null;

        List<RouteOption> routes = GetRoutesFromLocation(fromLocation);
        // RouteOption uses DestinationLocation object reference
        return routes.FirstOrDefault(r => r.DestinationLocation == toLocation);
    }

    /// <summary>
    /// Check if a route exists between two locations.
    /// HIGHLANDER: Accept Location objects
    /// </summary>
    public bool RouteExists(Location fromLocation, Location toLocation)
    {
        return GetRouteBetweenLocations(fromLocation, toLocation) != null;
    }

    /// <summary>
    /// Get all available routes from current location.
    /// HIGHLANDER: Use Location object directly
    /// </summary>
    public List<RouteOption> GetAvailableRoutesFromCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            return new List<RouteOption>();

        return GetRoutesFromLocation(currentLocation);
    }

}
