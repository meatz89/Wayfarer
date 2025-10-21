using System;
using System.Collections.Generic;
using System.Linq;

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
    public List<RouteOption> GetRoutesFromLocation(string venueId)
    {
        return _routeRepository.GetRoutesFromLocation(venueId).ToList();
    }

    /// <summary>
    /// Get a specific route between two locations.
    /// </summary>
    public RouteOption GetRouteBetweenLocations(string fromVenueId, string toVenueId)
    {
        List<RouteOption> routes = GetRoutesFromLocation(fromVenueId);
        // RouteOption uses DestinationLocationSpot to track where it goes
        // We need to find routes that go to any location in the target location
        return routes.FirstOrDefault(r =>
        {
            // Get the destination location and check if it belongs to the target location
            Location destSpot = _gameWorld.GetLocation(r.DestinationLocationSpot);
            return destSpot?.VenueId == toVenueId;
        });
    }

    /// <summary>
    /// Check if a route exists between two locations.
    /// </summary>
    public bool RouteExists(string fromVenueId, string toVenueId)
    {
        return GetRouteBetweenLocations(fromVenueId, toVenueId) != null;
    }

    /// <summary>
    /// Get all available routes from current location.
    /// </summary>
    public List<RouteOption> GetAvailableRoutesFromCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        string currentVenueId = player.CurrentLocation.VenueId;
        return GetRoutesFromLocation(currentVenueId);
    }

}
