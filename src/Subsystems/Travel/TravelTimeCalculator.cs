using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.TravelSubsystem
{
    /// <summary>
    /// Calculates travel times and costs between locations.
    /// </summary>
    public class TravelTimeCalculator
    {
        private readonly GameWorld _gameWorld;

        // Travel time matrix in segments
        private static readonly Dictionary<(string, string), int> TravelTimes = new Dictionary<(string, string), int>
        {
            // Your Room connections (very close to Market Square)
            { ("your_room", "market_square"), 1 },
            { ("market_square", "your_room"), 1 },
            
            // Market Square as central hub
            { ("market_square", "noble_district"), 2 },
            { ("noble_district", "market_square"), 2 },
            { ("market_square", "merchant_row"), 1 },
            { ("merchant_row", "market_square"), 1 },
            { ("market_square", "city_gates"), 2 },
            { ("city_gates", "market_square"), 2 },
            { ("market_square", "riverside"), 3 },
            { ("riverside", "market_square"), 3 },
            
            // Non-hub connections (longer)
            { ("noble_district", "merchant_row"), 3 },
            { ("merchant_row", "noble_district"), 3 },
            { ("noble_district", "city_gates"), 4 },
            { ("city_gates", "noble_district"), 4 },
            { ("noble_district", "riverside"), 4 },
            { ("riverside", "noble_district"), 4 },
            { ("merchant_row", "city_gates"), 2 },
            { ("city_gates", "merchant_row"), 2 },
            { ("merchant_row", "riverside"), 4 },
            { ("riverside", "merchant_row"), 4 },
            { ("city_gates", "riverside"), 2 },
            { ("riverside", "city_gates"), 2 },
            
            // Your Room to other locations (must go through Market Square)
            { ("your_room", "noble_district"), 3 },
            { ("noble_district", "your_room"), 3 },
            { ("your_room", "merchant_row"), 2 },
            { ("merchant_row", "your_room"), 2 },
            { ("your_room", "city_gates"), 3 },
            { ("city_gates", "your_room"), 3 },
            { ("your_room", "riverside"), 4 },
            { ("riverside", "your_room"), 4 },
        };

        public TravelTimeCalculator(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }

        /// <summary>
        /// Get base travel time between two locations in segments.
        /// </summary>
        public int GetBaseTravelTime(string fromLocationId, string toLocationId)
        {
            // Same location = no travel time
            if (fromLocationId == toLocationId)
            {
                return 0;
            }

            // Look up travel time
            (string fromLocationId, string toLocationId) key = (fromLocationId, toLocationId);
            if (TravelTimes.TryGetValue(key, out int time))
            {
                return time;
            }

            // No fallback - route must be defined
            Console.WriteLine($"[TravelTimeCalculator] Warning: No travel time defined for {fromLocationId} -> {toLocationId}");
            return 4; // Default to 4 segments if not found
        }

        /// <summary>
        /// Calculate actual travel time with transport method modifier.
        /// </summary>
        public int CalculateTravelTime(string fromLocationId, string toLocationId, TravelMethods transportMethod)
        {
            int baseTime = GetBaseTravelTime(fromLocationId, toLocationId);

            // Apply transport method modifier
            double modifier = GetTransportModifier(transportMethod);
            int actualTime = (int)(baseTime * modifier);

            // Apply weather effects if any
            actualTime = ApplyWeatherEffects(actualTime);

            return Math.Max(1, actualTime); // Minimum 1 segment travel time
        }

        /// <summary>
        /// Get transport method speed modifier.
        /// </summary>
        private double GetTransportModifier(TravelMethods transportMethod)
        {
            return transportMethod switch
            {
                TravelMethods.Walking => 1.0,      // Base speed
                TravelMethods.Horseback => 0.5,    // Twice as fast
                TravelMethods.Carriage => 0.7,     // Moderate speed boost
                TravelMethods.Cart => 1.3,          // Slower due to cargo
                TravelMethods.Boat => 0.8,          // Good for water routes
                _ => 1.0
            };
        }

        /// <summary>
        /// Apply weather effects to travel time.
        /// </summary>
        private int ApplyWeatherEffects(int baseTime)
        {
            WeatherCondition weather = _gameWorld.WorldState.CurrentWeather;

            return weather switch
            {
                WeatherCondition.Rain => (int)(baseTime * 1.2),      // 20% slower in rain
                WeatherCondition.Snow => (int)(baseTime * 1.5),      // 50% slower in snow
                WeatherCondition.Storm => (int)(baseTime * 2.0),     // Double time in storm
                _ => baseTime                                         // No effect
            };
        }

        /// <summary>
        /// Calculate coin cost for travel.
        /// </summary>
        public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
        {
            int baseCost = route.BaseCoinCost;

            // Apply transport method cost modifier
            baseCost = transportMethod switch
            {
                TravelMethods.Walking => 0,                  // Free
                TravelMethods.Horseback => baseCost * 2,     // More expensive
                TravelMethods.Carriage => baseCost * 3,      // Most expensive
                TravelMethods.Cart => baseCost,              // Standard cost
                TravelMethods.Boat => baseCost * 2,          // Moderate cost
                _ => baseCost
            };

            return baseCost;
        }

        /// <summary>
        /// Get all travel times from a specific location.
        /// </summary>
        public Dictionary<string, int> GetTravelTimesFrom(string locationId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (KeyValuePair<(string, string), int> kvp in TravelTimes)
            {
                if (kvp.Key.Item1 == locationId)
                {
                    result[kvp.Key.Item2] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if travel is possible given current time constraints.
        /// </summary>
        public bool CanTravelInTime(int travelTimeSegments, int availableSegments)
        {
            return travelTimeSegments <= availableSegments;
        }
    }
}