using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Services
{
    /// <summary>
    /// Procedural route generator using hex-based pathfinding.
    /// Generates routes between locations based on hex grid topology.
    /// Calculates route properties (danger, time, transport) from hex path terrain.
    ///
    /// Architecture: Routes connect Location → Location (never Venue → Venue)
    /// Venue membership determines travel cost rules (same venue = free, different venue = route)
    /// </summary>
    public class HexRouteGenerator
    {
        private readonly GameWorld _gameWorld;

        public HexRouteGenerator(GameWorld gameWorld)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        }

        /// <summary>
        /// Generate routes between a new location and all other locations with different venue membership
        /// Called when new location created to establish connectivity
        /// Routes only generated between locations in DIFFERENT venues
        /// </summary>
        public List<RouteOption> GenerateRoutesForNewLocation(Location newLocation)
        {
            if (newLocation == null)
                throw new ArgumentNullException(nameof(newLocation));

            if (!newLocation.HexPosition.HasValue)
                throw new ArgumentException($"Location '{newLocation.Id}' has no HexPosition - cannot generate routes", nameof(newLocation));

            List<RouteOption> generatedRoutes = new List<RouteOption>();

            // Find all existing locations in DIFFERENT venues
            List<Location> otherLocations = _gameWorld.Locations
                .Where(loc => loc.Id != newLocation.Id &&
                             loc.VenueId != newLocation.VenueId && // Different venue = requires route
                             loc.HexPosition.HasValue)
                .ToList();

            foreach (Location destination in otherLocations)
            {
                // Try all transport types (start with most restrictive for optimal pathing)
                TransportType[] transportPriority = new[]
                {
                    TransportType.Walking,   // Most universal
                    TransportType.Horseback, // Faster but terrain-restricted
                    TransportType.Cart,      // Efficient but limited
                    TransportType.Boat       // Water only
                };

                RouteOption route = null;

                foreach (TransportType transport in transportPriority)
                {
                    PathfindingResult pathResult = PathfindingService.FindPath(
                        newLocation.HexPosition.Value,
                        destination.HexPosition.Value,
                        _gameWorld.WorldHexGrid,
                        transport
                    );

                    if (pathResult.IsSuccess)
                    {
                        route = CreateRouteFromPath(
                            newLocation,
                            destination,
                            pathResult.Path,
                            pathResult.DangerRating,
                            pathResult.TotalCost,
                            transport
                        );

                        generatedRoutes.Add(route);
                        break; // Use first successful transport type
                    }
                }

                // No valid path found with any transport type
                if (route == null)
                {
                    // Log or track unreachable locations (optional)
                }
            }

            return generatedRoutes;
        }

        /// <summary>
        /// Generate all routes between existing locations
        /// Used for initial world setup or full route regeneration
        /// Only generates routes between locations in different venues
        /// </summary>
        public List<RouteOption> GenerateAllRoutes()
        {
            List<RouteOption> allRoutes = new List<RouteOption>();

            // Get all locations with hex positions
            List<Location> locationsWithPosition = _gameWorld.Locations
                .Where(loc => loc.HexPosition.HasValue)
                .ToList();

            // Generate routes between all pairs (avoiding duplicates)
            for (int i = 0; i < locationsWithPosition.Count; i++)
            {
                for (int j = i + 1; j < locationsWithPosition.Count; j++)
                {
                    Location loc1 = locationsWithPosition[i];
                    Location loc2 = locationsWithPosition[j];

                    // Only generate routes between different venues
                    if (loc1.VenueId == loc2.VenueId)
                        continue; // Same venue = instant travel, no route needed

                    // Try pathfinding with Walking (most universal)
                    PathfindingResult pathResult = PathfindingService.FindPath(
                        loc1.HexPosition.Value,
                        loc2.HexPosition.Value,
                        _gameWorld.WorldHexGrid,
                        TransportType.Walking
                    );

                    if (pathResult.IsSuccess)
                    {
                        // Create route from loc1 to loc2
                        RouteOption route = CreateRouteFromPath(
                            loc1,
                            loc2,
                            pathResult.Path,
                            pathResult.DangerRating,
                            pathResult.TotalCost,
                            TransportType.Walking
                        );

                        allRoutes.Add(route);
                    }
                }
            }

            return allRoutes;
        }

        /// <summary>
        /// Create RouteOption from pathfinding result
        /// Calculates route properties from hex path (danger, time, costs)
        /// </summary>
        private RouteOption CreateRouteFromPath(
            Location origin,
            Location destination,
            List<AxialCoordinates> hexPath,
            int dangerRating,
            float pathCost,
            TransportType transportType)
        {
            // Generate unique route ID
            string routeId = $"route_{origin.Id}_{destination.Id}";

            // Calculate time segments based on path length and terrain
            int timeSegments = CalculateTimeSegments(hexPath, transportType);

            // Calculate stamina cost based on path difficulty
            int staminaCost = CalculateStaminaCost(hexPath, transportType);

            // Calculate coin cost based on transport type
            int coinCost = CalculateCoinCost(transportType, hexPath.Count);

            // Create route
            RouteOption route = new RouteOption
            {
                Id = routeId,
                Name = $"{origin.Name} to {destination.Name}",
                OriginLocationSpot = origin.Id,
                DestinationLocationSpot = destination.Id,
                Method = ConvertTransportToTravelMethod(transportType),
                BaseCoinCost = coinCost,
                BaseStaminaCost = staminaCost,
                TravelTimeSegments = timeSegments,
                Description = GenerateRouteDescription(origin, destination, dangerRating, transportType),
                HexPath = hexPath,
                DangerRating = dangerRating,
                RouteType = DetermineRouteType(dangerRating)
            };

            return route;
        }

        /// <summary>
        /// Calculate time segments from hex path
        /// Based on specification: TimeSegments = PathLength × TerrainMultipliers
        /// </summary>
        private int CalculateTimeSegments(List<AxialCoordinates> hexPath, TransportType transportType)
        {
            if (hexPath == null || hexPath.Count == 0)
                return 1;

            float totalTime = 0;

            foreach (AxialCoordinates coords in hexPath)
            {
                Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
                if (hex != null)
                {
                    totalTime += GetTerrainTimeMultiplier(hex.Terrain, transportType);
                }
            }

            // Convert to time segments (round up, minimum 1)
            return Math.Max(1, (int)Math.Ceiling(totalTime / 3.0f)); // Divide by 3 for segment granularity
        }

        /// <summary>
        /// Calculate stamina cost based on terrain difficulty
        /// </summary>
        private int CalculateStaminaCost(List<AxialCoordinates> hexPath, TransportType transportType)
        {
            if (hexPath == null || hexPath.Count == 0)
                return 1;

            float totalStamina = 0;

            foreach (AxialCoordinates coords in hexPath)
            {
                Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
                if (hex != null)
                {
                    // Base stamina from terrain difficulty
                    float terrainStamina = GetTerrainTimeMultiplier(hex.Terrain, transportType) * 0.5f;
                    totalStamina += terrainStamina;
                }
            }

            return Math.Max(1, (int)Math.Ceiling(totalStamina));
        }

        /// <summary>
        /// Calculate coin cost based on transport type and distance
        /// </summary>
        private int CalculateCoinCost(TransportType transportType, int pathLength)
        {
            int baseCost = transportType switch
            {
                TransportType.Walking => 0,      // Free
                TransportType.Cart => 5,         // Moderate fee
                TransportType.Horseback => 10,   // Higher fee
                TransportType.Boat => 8,         // Moderate fee
                _ => 0
            };

            // Scale with distance
            return baseCost + (pathLength / 5); // +1 coin per 5 hexes
        }

        /// <summary>
        /// Get terrain time multiplier (same as pathfinding movement cost)
        /// From specification lines 181-187
        /// </summary>
        private float GetTerrainTimeMultiplier(TerrainType terrain, TransportType transportType)
        {
            switch (terrain)
            {
                case TerrainType.Plains:
                    return 1.0f;
                case TerrainType.Road:
                    return 0.8f;
                case TerrainType.Forest:
                    return transportType == TransportType.Cart ? 2.0f : 1.5f;
                case TerrainType.Mountains:
                    return 2.0f;
                case TerrainType.Swamp:
                    return 2.5f;
                case TerrainType.Water:
                    return 0.9f;
                case TerrainType.Impassable:
                    return float.PositiveInfinity;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Convert TransportType to TravelMethods enum
        /// </summary>
        private TravelMethods ConvertTransportToTravelMethod(TransportType transportType)
        {
            return transportType switch
            {
                TransportType.Walking => TravelMethods.Walking,
                TransportType.Cart => TravelMethods.Cart,
                TransportType.Horseback => TravelMethods.Horseback,
                TransportType.Boat => TravelMethods.Boat,
                _ => TravelMethods.Walking
            };
        }

        /// <summary>
        /// Determine route type based on danger rating
        /// For hex-generated routes, default to Knowledge (learn once, know forever)
        /// </summary>
        private RouteType DetermineRouteType(int dangerRating)
        {
            // Hex-generated routes are knowledge-based (discover once, available forever)
            // Alternative types (Seal, Service) are for manually-authored special routes
            return RouteType.Knowledge;
        }

        /// <summary>
        /// Generate descriptive text for route
        /// </summary>
        private string GenerateRouteDescription(Location origin, Location destination, int dangerRating, TransportType transportType)
        {
            string safetyDesc = dangerRating < 10 ? "safe" :
                              dangerRating < 30 ? "moderately dangerous" :
                              "very dangerous";

            string transportDesc = transportType switch
            {
                TransportType.Walking => "on foot",
                TransportType.Cart => "by cart",
                TransportType.Horseback => "on horseback",
                TransportType.Boat => "by boat",
                _ => "by travel"
            };

            return $"Travel from {origin.Name} to {destination.Name} {transportDesc}. This route is {safetyDesc}.";
        }
    }
}
