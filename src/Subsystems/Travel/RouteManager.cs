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
    /// </summary>
    public List<RouteOption> GetRoutesFromLocation(string locationId)
    {
        return _routeRepository.GetRoutesFromLocation(locationId).ToList();
    }

    /// <summary>
    /// Get a specific route between two locations.
    /// </summary>
    public RouteOption GetRouteBetweenLocations(string fromLocationId, string toLocationId)
    {
        List<RouteOption> routes = GetRoutesFromLocation(fromLocationId);
        // RouteOption uses DestinationLocation for exact destination
        return routes.FirstOrDefault(r => r.DestinationLocation == toLocationId);
    }

    /// <summary>
    /// Check if a route exists between two locations.
    /// </summary>
    public bool RouteExists(string fromLocationId, string toLocationId)
    {
        return GetRouteBetweenLocations(fromLocationId, toLocationId) != null;
    }

    /// <summary>
    /// Get all available routes from current location.
    /// </summary>
    public List<RouteOption> GetAvailableRoutesFromCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            return new List<RouteOption>();

        string currentVenueId = currentLocation.VenueId;
        return GetRoutesFromLocation(currentVenueId);
    }

}
