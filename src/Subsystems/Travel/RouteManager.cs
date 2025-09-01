using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.TravelSubsystem
{
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
            var routes = GetRoutesFromLocation(fromLocationId);
            // RouteOption uses DestinationLocationSpot to track where it goes
            // We need to find routes that go to any spot in the target location
            return routes.FirstOrDefault(r => 
            {
                // Get the destination spot and check if it belongs to the target location
                var destSpot = _gameWorld.WorldState.locations
                    ?.SelectMany(l => l.Spots ?? new List<LocationSpot>())
                    .FirstOrDefault(s => s.SpotID == r.DestinationLocationSpot);
                return destSpot?.LocationId == toLocationId;
            });
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
            var player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return new List<RouteOption>();
            }
            return GetRoutesFromLocation(currentLocationId);
        }
        
        /// <summary>
        /// Check if player has discovered a specific route.
        /// </summary>
        public bool IsRouteDiscovered(string routeId)
        {
            var player = _gameWorld.GetPlayer();
            return player.DiscoveredRoutes?.Contains(routeId) ?? false;
        }
        
        /// <summary>
        /// Mark a route as discovered by the player.
        /// </summary>
        public void DiscoverRoute(string routeId)
        {
            var player = _gameWorld.GetPlayer();
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
            var player = _gameWorld.GetPlayer();
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
}