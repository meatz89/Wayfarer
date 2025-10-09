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
        // We need to find routes that go to any spot in the target location
        return routes.FirstOrDefault(r =>
        {
            // Get the destination spot and check if it belongs to the target location
            LocationSpot destSpot = _gameWorld.GetSpot(r.DestinationLocationSpot);
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
        string currentVenueId = player.CurrentLocationSpot?.VenueId;
        if (currentVenueId == null)
        {
            return new List<RouteOption>();
        }
        return GetRoutesFromLocation(currentVenueId);
    }

    /// <summary>
    /// Check if player has discovered a specific route.
    /// </summary>
    public bool IsRouteDiscovered(string routeId)
    {
        Player player = _gameWorld.GetPlayer();
        return player.DiscoveredRoutes?.Contains(routeId) ?? false;
    }

    /// <summary>
    /// Mark a route as discovered by the player.
    /// </summary>
    public void DiscoverRoute(string routeId)
    {
        Player player = _gameWorld.GetPlayer();
        if (player.DiscoveredRoutes == null)
        {
            player.DiscoveredRoutes = new List<string>();
        }

        if (!player.DiscoveredRoutes.Contains(routeId))
        {
            player.DiscoveredRoutes.Add(routeId);
            Console.WriteLine($"[RouteManager] Player discovered route: {routeId}");
        }
    }

    /// <summary>
    /// Get all discovered routes for the player.
    /// </summary>
    public List<RouteOption> GetDiscoveredRoutes()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.DiscoveredRoutes == null || !player.DiscoveredRoutes.Any())
        {
            return new List<RouteOption>();
        }

        return player.DiscoveredRoutes
            .Select(routeId => _routeRepository.GetRouteById(routeId))
            .Where(route => route != null)
            .ToList();
    }
}
