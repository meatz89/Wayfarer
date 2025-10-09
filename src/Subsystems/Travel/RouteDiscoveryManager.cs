using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages route discovery mechanics and exploration.
/// </summary>
public class RouteDiscoveryManager
{
    private readonly RouteManager _routeManager;
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    public RouteDiscoveryManager(
        RouteManager routeManager,
        GameWorld gameWorld,
        MessageSystem messageSystem)
    {
        _routeManager = routeManager;
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Attempt to discover a new route.
    /// </summary>
    public bool AttemptRouteDiscovery(string fromVenueId, string toVenueId)
    {
        // Check if route exists
        RouteOption route = _routeManager.GetRouteBetweenLocations(fromVenueId, toVenueId);
        if (route == null)
        {
            _messageSystem.AddSystemMessage(
                "No route exists between these locations.",
                SystemMessageTypes.Warning);
            return false;
        }

        // Check if already discovered
        if (_routeManager.IsRouteDiscovered(route.Id))
        {
            _messageSystem.AddSystemMessage(
                "You already know this route.",
                SystemMessageTypes.Info);
            return false;
        }

        // Discover the route
        _routeManager.DiscoverRoute(route.Id);

        _messageSystem.AddSystemMessage(
            $"🗺️ Discovered new route: {route.Name}",
            SystemMessageTypes.Success);

        return true;
    }

    /// <summary>
    /// Get all undiscovered routes from current location.
    /// </summary>
    public List<RouteOption> GetUndiscoveredRoutesFromCurrentLocation()
    {
        List<RouteOption> allRoutes = _routeManager.GetAvailableRoutesFromCurrentLocation();
        return allRoutes.Where(r => !_routeManager.IsRouteDiscovered(r.Id)).ToList();
    }

    /// <summary>
    /// Get discovery progress statistics.
    /// </summary>
    public DiscoveryProgressInfo GetDiscoveryProgress()
    {
        Player player = _gameWorld.GetPlayer();
        string currentVenueId = player.CurrentLocationSpot?.VenueId;
        if (currentVenueId == null)
        {
            return new DiscoveryProgressInfo(0, 0);
        }

        List<RouteOption> allRoutes = _routeManager.GetRoutesFromLocation(currentVenueId);
        int discoveredCount = allRoutes.Count(r => _routeManager.IsRouteDiscovered(r.Id));

        return new DiscoveryProgressInfo(discoveredCount, allRoutes.Count);
    }

    /// <summary>
    /// Discover all basic routes (called during game initialization).
    /// </summary>
    public void DiscoverBasicRoutes()
    {
        // Basic routes that are always known
        List<string> basicRouteIds = new List<string>
        {
            "your_room_to_market_square",
            "market_square_to_your_room"
        };

        foreach (string routeId in basicRouteIds)
        {
            _routeManager.DiscoverRoute(routeId);
        }

        Console.WriteLine("[RouteDiscoveryManager] Basic routes discovered");
    }

    /// <summary>
    /// Check if player can explore from current location.
    /// </summary>
    public bool CanExploreFromCurrentLocation()
    {
        return GetUndiscoveredRoutesFromCurrentLocation().Any();
    }
}
