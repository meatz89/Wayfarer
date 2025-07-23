using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of IRouteRepository
    /// </summary>
    public class RouteRepositoryImpl : IRouteRepository
    {
        private readonly IWorldStateAccessor _worldState;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<RouteRepositoryImpl> _logger;

        public RouteRepositoryImpl(
            IWorldStateAccessor worldState,
            ILocationRepository locationRepository,
            ILogger<RouteRepositoryImpl> logger)
        {
            _worldState = worldState ?? throw new System.ArgumentNullException(nameof(worldState));
            _locationRepository = locationRepository ?? throw new System.ArgumentNullException(nameof(locationRepository));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public IEnumerable<RouteOption> GetRoutesFromLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Enumerable.Empty<RouteOption>();
            }

            var allRoutes = new List<RouteOption>();

            // Get routes from location's connections
            var location = _locationRepository.GetById(locationId);
            if (location?.Connections != null)
            {
                foreach (var connection in location.Connections)
                {
                    foreach (var routeType in connection.RouteTypes)
                    {
                        allRoutes.Add(new RouteOption
                        {
                            Id = $"{locationId}_to_{connection.DestinationId}_{routeType}",
                            StartLocationId = locationId,
                            DestinationId = connection.DestinationId,
                            RouteType = routeType,
                            BaseHoursCost = connection.BaseHoursCost,
                            RequiredEquipment = GetRequiredEquipmentForRoute(routeType)
                        });
                    }
                }
            }

            _logger.LogDebug($"Found {allRoutes.Count} routes from location '{locationId}'");
            return allRoutes;
        }

        public bool IsRouteBlocked(string routeId)
        {
            // This would need access to WorldState's route blocking system
            // For now, return false as default
            _logger.LogDebug($"Checking if route '{routeId}' is blocked");
            return false;
        }

        public WeatherCondition GetCurrentWeather()
        {
            // This would need access to WorldState's weather system
            return WeatherCondition.Clear;
        }

        public IEnumerable<RouteOption> GetAvailableRoutes(string fromLocationId, Player player)
        {
            var allRoutes = GetRoutesFromLocation(fromLocationId);
            
            // Filter routes based on player capabilities
            return allRoutes.Where(route => 
            {
                // Check if route is blocked
                if (IsRouteBlocked(route.Id))
                {
                    _logger.LogDebug($"Route '{route.Id}' is blocked");
                    return false;
                }

                // Check weather conditions
                if (!IsRouteAvailableInWeather(route, GetCurrentWeather()))
                {
                    _logger.LogDebug($"Route '{route.Id}' not available in current weather");
                    return false;
                }

                // Check equipment requirements
                if (!PlayerHasRequiredEquipment(route, player))
                {
                    _logger.LogDebug($"Player lacks equipment for route '{route.Id}'");
                    return false;
                }

                return true;
            });
        }

        public bool PlayerHasRequiredEquipment(RouteOption route, Player player)
        {
            if (route.RequiredEquipment == null || !route.RequiredEquipment.Any())
            {
                return true;
            }

            // Check if player has all required equipment
            foreach (var required in route.RequiredEquipment)
            {
                if (!player.Inventory.Items.Any(item => item.Categories.Contains(required)))
                {
                    return false;
                }
            }

            return true;
        }

        private List<ItemCategory> GetRequiredEquipmentForRoute(RouteType routeType)
        {
            return routeType switch
            {
                RouteType.River => new List<ItemCategory> { ItemCategory.Water_Transport },
                RouteType.Mountain => new List<ItemCategory> { ItemCategory.Climbing_Equipment },
                RouteType.Restricted => new List<ItemCategory> { ItemCategory.Special_Access },
                _ => new List<ItemCategory>()
            };
        }

        private bool IsRouteAvailableInWeather(RouteOption route, WeatherCondition weather)
        {
            // Mountain routes blocked in storms
            if (route.RouteType == RouteType.Mountain && weather == WeatherCondition.Storm)
            {
                return false;
            }

            // River routes blocked when frozen
            if (route.RouteType == RouteType.River && weather == WeatherCondition.Snow)
            {
                return false;
            }

            return true;
        }
    }
}