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
    /// PHASE 6D: Accept Location objects instead of IDs
    /// </summary>
    public RouteOption GetRouteBetweenLocations(Location fromLocation, Location toLocation)
    {
        if (fromLocation == null || toLocation == null)
            return null;

        List<RouteOption> routes = GetRoutesFromLocation(fromLocation.Name);
        // RouteOption uses DestinationLocation object reference
        return routes.FirstOrDefault(r => r.DestinationLocation == toLocation);
    }

    /// <summary>
    /// Check if a route exists between two locations.
    /// PHASE 6D: Accept Location objects instead of IDs
    /// </summary>
    public bool RouteExists(Location fromLocation, Location toLocation)
    {
        return GetRouteBetweenLocations(fromLocation, toLocation) != null;
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

        // ADR-007: Use Venue.Name instead of deleted VenueId
        string currentVenueId = currentLocation.Venue.Name;
        return GetRoutesFromLocation(currentVenueId);
    }

}
