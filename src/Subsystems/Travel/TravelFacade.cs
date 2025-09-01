using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.TravelSubsystem
{
    /// <summary>
    /// Public facade for all travel-related operations.
    /// Single entry point for travel, routes, and exploration.
    /// </summary>
    public class TravelFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly RouteManager _routeManager;
        private readonly RouteDiscoveryManager _routeDiscoveryManager;
        private readonly PermitValidator _permitValidator;
        private readonly TravelTimeCalculator _travelTimeCalculator;
        private readonly MessageSystem _messageSystem;

        public TravelFacade(
            GameWorld gameWorld,
            RouteManager routeManager,
            RouteDiscoveryManager routeDiscoveryManager,
            PermitValidator permitValidator,
            TravelTimeCalculator travelTimeCalculator,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _routeManager = routeManager;
            _routeDiscoveryManager = routeDiscoveryManager;
            _permitValidator = permitValidator;
            _travelTimeCalculator = travelTimeCalculator;
            _messageSystem = messageSystem;
        }

        // ========== ROUTE OPERATIONS ==========

        public List<RouteOption> GetAvailableRoutesFromCurrentLocation()
        {
            return _routeManager.GetAvailableRoutesFromCurrentLocation();
        }

        /// <summary>
        /// Get travel destinations with full view model data for UI
        /// </summary>
        public List<TravelDestinationViewModel> GetTravelDestinations()
        {
            List<RouteOption> routes = GetAvailableRoutesFromCurrentLocation();
            List<TravelDestinationViewModel> destinations = new List<TravelDestinationViewModel>();

            foreach (RouteOption route in routes)
            {
                // Extract location ID from destination spot (format: locationId.spotName)
                string locationId = route.DestinationLocationSpot.Split('.')[0];
                Location? destination = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
                if (destination != null)
                {
                    bool canTravel = IsRouteDiscovered(route.Id);

                    destinations.Add(new TravelDestinationViewModel
                    {
                        LocationId = destination.Id,
                        LocationName = destination.Name,
                        Description = destination.Description ?? "",
                        CanTravel = canTravel,
                        CannotTravelReason = !canTravel ? "Route not discovered" : null,
                        MinimumCost = CalculateTravelCost(route, TravelMethods.Walking),
                        MinimumTime = route.TravelTimeMinutes,
                        IsCurrent = false,
                        Routes = new List<TravelRouteViewModel>() // This would be populated by a more detailed method
                    });
                }
            }

            return destinations;
        }

        public List<RouteOption> GetDiscoveredRoutes()
        {
            return _routeManager.GetDiscoveredRoutes();
        }

        public RouteOption GetRouteBetweenLocations(string fromLocationId, string toLocationId)
        {
            return _routeManager.GetRouteBetweenLocations(fromLocationId, toLocationId);
        }

        public bool IsRouteDiscovered(string routeId)
        {
            return _routeManager.IsRouteDiscovered(routeId);
        }

        // ========== TRAVEL OPERATIONS ==========

        public bool CanTravelTo(string locationId)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return false;
            }

            // Check if route exists
            RouteOption route = GetRouteBetweenLocations(currentLocationId, locationId);
            if (route == null)
            {
                return false;
            }

            // Routes are always available - no discovery mechanic

            // Check permits
            if (!_permitValidator.HasRequiredPermit(route))
            {
                return false;
            }

            return true;
        }

        public TravelResult TravelTo(string locationId, TravelMethods transportMethod)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = "Current location is unknown"
                };
            }

            // Get route
            RouteOption route = GetRouteBetweenLocations(currentLocationId, locationId);
            if (route == null)
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = "No route exists to that location"
                };
            }

            // Routes are always available - no discovery mechanic

            // Check permits
            if (!_permitValidator.HasRequiredPermit(route))
            {
                List<string> missingPermits = _permitValidator.GetMissingPermits(route);
                return new TravelResult
                {
                    Success = false,
                    Reason = $"Missing required permits: {string.Join(", ", missingPermits)}"
                };
            }

            // Check transport compatibility
            if (!_permitValidator.IsTransportCompatible(route, transportMethod))
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = $"{transportMethod} cannot be used on this route"
                };
            }

            // Calculate time and cost
            int travelTime = _travelTimeCalculator.CalculateTravelTime(currentLocationId, locationId, transportMethod);
            int coinCost = _travelTimeCalculator.CalculateTravelCost(route, transportMethod);

            // Check if player can afford
            if (coinCost > 0 && _gameWorld.GetPlayer().Coins < coinCost)
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = $"Not enough coins. Need {coinCost}, have {_gameWorld.GetPlayer().Coins}"
                };
            }

            // Return travel information for GameFacade to execute
            // GameFacade will handle coin deduction and location update
            return new TravelResult
            {
                Success = true,
                TravelTimeMinutes = travelTime,
                CoinCost = coinCost,
                RouteId = route.Id,
                DestinationId = locationId,
                TransportMethod = transportMethod
            };
        }

        // ========== DISCOVERY OPERATIONS ==========

        public bool AttemptRouteDiscovery(string toLocationId)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return false;
            }
            return _routeDiscoveryManager.AttemptRouteDiscovery(currentLocationId, toLocationId);
        }

        public List<RouteOption> GetUndiscoveredRoutes()
        {
            return _routeDiscoveryManager.GetUndiscoveredRoutesFromCurrentLocation();
        }

        public DiscoveryProgressInfo GetDiscoveryProgress()
        {
            return _routeDiscoveryManager.GetDiscoveryProgress();
        }

        public bool CanExploreFromCurrentLocation()
        {
            return _routeDiscoveryManager.CanExploreFromCurrentLocation();
        }

        // ========== PERMIT OPERATIONS ==========

        public bool HasRequiredPermit(RouteOption route)
        {
            return _permitValidator.HasRequiredPermit(route);
        }

        public List<string> GetMissingPermits(RouteOption route)
        {
            return _permitValidator.GetMissingPermits(route);
        }

        public string GetAccessRequirementDescription(RouteOption route)
        {
            return _permitValidator.GetAccessRequirementDescription(route);
        }

        public bool IsTransportCompatible(RouteOption route, TravelMethods transportMethod)
        {
            return _permitValidator.IsTransportCompatible(route, transportMethod);
        }

        // ========== TIME CALCULATIONS ==========

        public int CalculateTravelTime(string toLocationId, TravelMethods transportMethod)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return 0;
            }
            return _travelTimeCalculator.CalculateTravelTime(currentLocationId, toLocationId, transportMethod);
        }

        public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
        {
            return _travelTimeCalculator.CalculateTravelCost(route, transportMethod);
        }

        public Dictionary<string, int> GetTravelTimesFromCurrentLocation()
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return new Dictionary<string, int>();
            }
            return _travelTimeCalculator.GetTravelTimesFrom(currentLocationId);
        }

        // ========== TRANSPORT METHODS ==========

        public List<TravelMethods> GetAvailableTransportMethods()
        {
            Player player = _gameWorld.GetPlayer();
            List<TravelMethods> methods = new List<TravelMethods> { TravelMethods.Walking }; // Always can walk

            // Check for unlocked transport methods
            if (player.UnlockedTravelMethods != null)
            {
                foreach (string method in player.UnlockedTravelMethods)
                {
                    if (Enum.TryParse<TravelMethods>(method, out TravelMethods travelMethod))
                    {
                        methods.Add(travelMethod);
                    }
                }
            }

            return methods.Distinct().ToList();
        }

        public void UnlockTransportMethod(TravelMethods method)
        {
            Player player = _gameWorld.GetPlayer();
            if (player.UnlockedTravelMethods == null)
            {
                player.UnlockedTravelMethods = new List<string>();
            }

            string methodName = method.ToString();
            if (!player.UnlockedTravelMethods.Contains(methodName))
            {
                player.UnlockedTravelMethods.Add(methodName);
                _messageSystem.AddSystemMessage(
                    $"🎯 Unlocked new transport method: {method}",
                    SystemMessageTypes.Success);
            }
        }
    }

    /// <summary>
    /// Result of a travel attempt.
    /// </summary>
    public class TravelResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; }
        public int TravelTimeMinutes { get; set; }
        public int CoinCost { get; set; }
        public string RouteId { get; set; }
        public string DestinationId { get; set; }
        public TravelMethods TransportMethod { get; set; }
    }
}